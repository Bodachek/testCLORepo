import * as React from 'react';
import * as ReactDom from 'react-dom';
import * as lodash from "lodash";
import { Version } from '@microsoft/sp-core-library';
import {
  BaseClientSideWebPart,
  IPropertyPaneConfiguration
} from '@microsoft/sp-webpart-base';

import * as strings from 'CustomLearningAdminWebPartStrings';
import CustomLearningAdmin from './components/CustomLearningAdmin';
import { ICustomConfig, CustomConfig, IMetadata, IPlaylist, IAsset, ICustomSubCategories } from '../common/models/Models';
import { Roles } from '../common/models/Enums';
import { Logger, ConsoleListener, LogLevel } from '@pnp/logging';
import { sp } from '@pnp/sp';
import { InitService, IInitService } from '../common/services/InitService';
import { DataService, IDataService } from '../common/services/DataService';
import Error from "../common/components/Atoms/Error";
import { ICustomDataService, CustomDataService } from '../common/services/CustomDataService';

export interface ICustomLearningAdminWebPartProps {
}

export default class CustomLearningAdminWebPart extends BaseClientSideWebPart<ICustomLearningAdminWebPartProps> {
  private LOG_SOURCE: string = "CustomLearningAdminWebPart";
  private _initService: IInitService;
  private _validConfig: boolean = false;
  private _dataService: IDataService;
  private _userSecurity: string;
  private _customConfig: ICustomConfig = new CustomConfig();
  private _metadata: IMetadata;
  private _playlists: IPlaylist[];
  private _assets: IAsset[];
  private _appPartPage: boolean = false;
  private _cdnBase: string = "";
  private _customService: ICustomDataService;
  private _customSubCategories: ICustomSubCategories;

  public async onInit(): Promise<void> {
    //Initialize PnPLogger
    Logger.subscribe(new ConsoleListener());
    Logger.activeLogLevel = LogLevel.Info;
    //Initialize PnPJs
    sp.setup({
      spfxContext: this.context
    });

    try {
      this._initService = new InitService();
      await this._initService.initialize();
      //Validate in custom learning site
      if (this.context.pageContext.web.absoluteUrl.toLowerCase() != this._initService.LearningSite.toLowerCase()) {
        Logger.write(`Microsoft Custom Learning Admin is not in the web defined by the tenant property MicrosoftCustomLearningSite: '${this._initService.LearningSite}'. If the tenant property is correct than this web part cannot run in this web.`, LogLevel.Error);
        return;
      }
      this._userSecurity = await this._initService.getUser();
      this._validConfig = await this._initService.validateLists((this._userSecurity === Roles.Owners));
      if (this._validConfig) {
        this._dataService = new DataService(this.context.httpClient, this._initService.LearningSite);
        this._customConfig = await this._dataService.getCustomConfig();
        if (!this._customConfig){
          this._customConfig = new CustomConfig();
        }
        this._customConfig = await this._dataService.refreshConfig(this._customConfig);
        this._customSubCategories = this._dataService.customSubCategories;
        this._metadata = this._dataService.metadata;
        this._playlists = this._dataService.playlistsAll;
        this._assets = this._dataService.assetsAll;
        //Determine if on an app part page
        this._appPartPage = (document.getElementById("spPageCanvasContent") == null);
        this._cdnBase = this._dataService.cdnBase;
        Logger.write(`Initialized Microsoft Custom Learning Admin - Tenant: ${this.context.pageContext.aadInfo._tenantId}`, LogLevel.Info);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (onInit) -- Could not start web part.`, LogLevel.Error);
    }
  }

  public async render(): Promise<void> {
    let element;
    try {
      if (this._validConfig) {
        if (this._userSecurity !== Roles.Visitors) {
          //Render web part
          element = React.createElement(
            CustomLearningAdmin,
            {
              config: this._customConfig,
              customSubCategories: this._customSubCategories,
              cdnBase: this._cdnBase,
              categories: this._metadata.Categories,
              technologies: this._metadata.Technologies,
              allTechnologies: this._metadata.Technologies,
              playlistsAll: this._playlists,
              assetsAll: this._assets,
              saveConfig: this.saveConfig,
              upsertSubCategories: this.upsertSubCategories,
              upsertAsset: this.upsertAsset,
              upsertPlaylist: this.upsertPlaylist,
              appPartPage: this._appPartPage,
              deletePlaylist: this.deletePlaylist
            }
          );
        } else {
          element = React.createElement(
            Error,
            {
              message: `You do not have rights to administrate Custom Learning for Office 365.`
            }
          );
        }
      } else {
        element = React.createElement(
          Error,
          {
            message: `The ${this.LOG_SOURCE} has not been properly configured and you do not have Owner rights to complete the operation. Please contact the site administrator.`
          }
        );
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
    }
    ReactDom.render(element, this.domElement);
    return;
  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse('1.0');
  }

  private saveConfig = async (newConfig: ICustomConfig): Promise<void> => {
    this._customConfig = await this._dataService.refreshConfig(newConfig);
    this._metadata = this._dataService.metadata;
    this.render();
    return;
  }

  private upsertSubCategories = async (newSubCategories: ICustomSubCategories): Promise<string> => {
    try {
      this._customService = this._customService || new CustomDataService(this._dataService.learningWeb);
      let saveSubCategories: string;
      if (newSubCategories.Id === "0") {
        let savePlaylistVal = await this._customService.createSubCategories(newSubCategories);
        saveSubCategories = savePlaylistVal.toString();
        newSubCategories.Id = saveSubCategories;
      } else {
        saveSubCategories = await this._customService.modifySubCategories(newSubCategories);
      }
      if (saveSubCategories !== "0") {
        this._customSubCategories = newSubCategories;
        this.saveConfig(lodash.cloneDeep(this._customConfig));
      }

      return saveSubCategories;
    }
    catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertSubCategories)`, LogLevel.Error);
      return "0";
    }
  }

  private upsertPlaylist = async (playlist: IPlaylist): Promise<string> => {
    try {
      this._customService = this._customService || new CustomDataService(this._dataService.learningWeb);
      let savePlaylist: string;
      if (playlist.Id === "0") {
        let savePlaylistVal = await this._customService.createPlaylist(playlist);
        savePlaylist = savePlaylistVal.toString();
      } else {
        savePlaylist = await this._customService.modifyPlaylist(playlist);
      }
      if (savePlaylist !== "0") {
        //Refresh playlists
        this._playlists = await this._dataService.refreshPlaylistsAll(true);
        //Reset config and render
        this._customConfig = await this._dataService.refreshConfigCustomOnly(this._customConfig);
        this.render();
      }

      return savePlaylist;
    }
    catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertPlaylist)`, LogLevel.Error);
      return "0";
    }
  }

  private deletePlaylist = async (playlistId: string): Promise<void> => {
    try {
      this._customService = this._customService || new CustomDataService(this._dataService.learningWeb);
      let deleteResult = await this._customService.deletePlaylist(playlistId);
      if (deleteResult) {
        //Refresh playlists
        this._playlists = await this._dataService.refreshPlaylistsAll(true);
        //Reset config and render
        this._customConfig = await this._dataService.refreshConfigCustomOnly(this._customConfig);
        this.render();
      }
    }
    catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (deletePlaylist)`, LogLevel.Error);
    }
    return;
  }

  private upsertAsset = async (asset: IAsset): Promise<string> => {
    try {
      this._customService = this._customService || new CustomDataService(this._dataService.learningWeb);
      let saveAsset: string;
      if (asset.Id === "0") {
        let saveAssetVal = await this._customService.createAsset(asset);
        saveAsset = saveAssetVal.toString();
      } else {
        saveAsset = await this._customService.modifyAsset(asset);
      }
      if (saveAsset !== "0") {
        //Refresh assets
        this._assets = await this._dataService.refreshAssetsAll(true);
        //Reset config and render
        this._customConfig = await this._dataService.refreshConfigCustomOnly(this._customConfig);
        this.render();
      }
      return saveAsset;
    }
    catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertPlaylist)`, LogLevel.Error);
      return "0";
    }
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    return {
      pages: [
        {
          header: {
            description: strings.PropertyPaneDescription
          },
          groups: [
            {
              groupName: strings.BasicGroupName,
              groupFields: [
              ]
            }
          ]
        }
      ]
    };
  }
}
