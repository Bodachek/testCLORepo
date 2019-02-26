import * as React from "react";
import * as ReactDom from "react-dom";
import { Version, DisplayMode } from "@microsoft/sp-core-library";
import {
  BaseClientSideWebPart,
  IPropertyPaneConfiguration,
  PropertyPaneLabel,
  IPropertyPaneDropdownOption,
  PropertyPaneTextField
} from "@microsoft/sp-webpart-base";

import { CalloutTriggers } from '@pnp/spfx-property-controls/lib/PropertyFieldHeader';
import { PropertyFieldDropdownWithCallout } from '@pnp/spfx-property-controls/lib/PropertyFieldDropdownWithCallout';

import { sp } from '@pnp/sp';
import "@pnp/polyfill-ie11";
import * as lodash from 'lodash';
import { Logger, LogLevel, ConsoleListener } from "@pnp/logging";
import { UrlQueryParameterCollection } from '@microsoft/sp-core-library';

import * as strings from "CustomLearningWebPartStrings";
import CustomLearning from "./components/CustomLearning";
import { ICustomConfig, CustomConfig, ICategory, ISubCategory, IPlaylist } from "../common/models/Models";
import { Roles, Templates, WebpartMode, PropertyPaneFilters, CustomWebpartSource } from "../common/models/Enums";
import CacheController from "../common/services/CacheController";
import Error from "../common/components/Atoms/Error";
import { AppInsights } from "applicationinsights-js";
import * as shajs from 'sha.js';

export interface ICustomLearningWebPartProps {
  webpartMode: string;
  defaultFilter: string;
  defaultCategory: string;
  defaultSubCategory: string;
  defaultPlaylist: string;
  defaultAsset: string;
  title: string;
}

export default class CustomLearningWebPart extends BaseClientSideWebPart<ICustomLearningWebPartProps> {
  private LOG_SOURCE: string = "CustomLearningWebPart";
  private _userSecurity: string;
  private _customConfig: ICustomConfig = new CustomConfig();
  private _validConfig: boolean = false;

  private _webpartMode: string = "";
  private _startType: string = "";
  private _startLocation: string = "";

  private _ppWebpartMode: IPropertyPaneDropdownOption[];
  private _ppCategory: IPropertyPaneDropdownOption[];
  private _ppSubCategory: IPropertyPaneDropdownOption[];
  private _ppPlaylist: IPropertyPaneDropdownOption[];
  private _ppFilters: IPropertyPaneDropdownOption[];
  private _ppAssets: IPropertyPaneDropdownOption[];

  private _baseAdminUrl: string = "";
  private _baseViewerUrl: string = "";
  private _appPartPage: boolean = false;

  //Get the values from the query string if necessary
  private _queryParms: UrlQueryParameterCollection = new UrlQueryParameterCollection(window.location.href);
  private _urlWebpartMode: string = this._queryParms.getValue("webpartmode");
  private _urlCategory: string = this._queryParms.getValue("category");
  private _urlSubCategory: string = this._queryParms.getValue("subcategory");
  private _urlPlaylist: string = this._queryParms.getValue("playlist");
  private _startAsset: string = this._queryParms.getValue("asset");

  public async onInit(): Promise<void> {
    //Initialize PnPLogger
    Logger.subscribe(new ConsoleListener());
    Logger.activeLogLevel = LogLevel.Info;
    //Initialize PnPJs
    sp.setup({
      spfxContext: this.context
    });

    try {
      let cacheController = CacheController.getInstance(this.context.httpClient);
      let ready = await cacheController.IsReady();
      if (ready && cacheController.IsValid) {
        this._validConfig = cacheController.IsValid;
        this._baseAdminUrl = `${cacheController.LearningSite}/SitePages/CustomLearningAdmin.aspx`;
        this._baseViewerUrl = `${cacheController.LearningSite}/SitePages/CustomLearningViewer.aspx`;
        this._userSecurity = cacheController.UserSecurity;
        this._customConfig = cacheController.CustomConfig;
        //Filter Category/SubCategory for Security
        this._customConfig.CachedMetadata.Categories = this.getSecurityTrimmedCategories(this._customConfig.CachedMetadata.Categories);

        //Initialize App Insights
        if (cacheController.TelemetryOn) {
          AppInsights.downloadAndSetup({ instrumentationKey: this._customConfig.CachedMetadata.Telemetry.AppInsightKey });
          let userHash = shajs('sha256').update(this.context.pageContext.user.loginName).digest('hex');
          AppInsights.setAuthenticatedUserContext(userHash);
          let tenant = this.context.pageContext.aadInfo._tenantId._guid;
          AppInsights.trackEvent(this.LOG_SOURCE, { tenant: tenant }, {});
        }

        //Determine if on an app part page
        this._appPartPage = (document.getElementById("spPageCanvasContent") == null);
      }
      Logger.write(`Initialized Microsoft Custom Learning - Tenant: ${this.context.pageContext.aadInfo._tenantId}`, LogLevel.Info);

    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render) -- Could not start web part.`, LogLevel.Error);
    }
  }

  public async render(): Promise<void> {
    let element;
    if (this._validConfig) {
      //Update startType and startLocation if changed.
      if (this.properties.webpartMode !== "" && this.properties.webpartMode !== this._webpartMode) {
        this._webpartMode = this.properties.webpartMode;
      }

      if (this.properties.defaultCategory !== "" && this.properties.defaultCategory !== this._startLocation) {
        this._startType = Templates.Category;
        this._startLocation = this.properties.defaultCategory;
      }

      if (this.properties.defaultSubCategory !== "" && this.properties.defaultSubCategory !== this._startLocation) {
        this._startType = Templates.SubCategory;
        this._startLocation = this.properties.defaultSubCategory;
      }
      if (this.properties.defaultPlaylist !== "" && this.properties.defaultPlaylist !== this._startLocation) {
        this._startType = Templates.Playlist;
        this._startLocation = this.properties.defaultPlaylist;
      }
      if (this.properties.defaultAsset !== "" && this.properties.defaultAsset !== this._startAsset) {
        this._startAsset = this.properties.defaultAsset;
      }
      if (this.properties.defaultCategory === "" && this.properties.defaultSubCategory === "" && this.properties.defaultPlaylist === "") {
        this._startType = "";
        this._startLocation = "";
      }

      //Override if the query string parameters are set. But we don't want to do this if we are in edit mode.
      if (this.displayMode != DisplayMode.Edit) {
        //Set Webpart mode via query string
        if ((this._urlWebpartMode) && (this._urlWebpartMode !== "")) {
          this._webpartMode = this._urlWebpartMode;
        }

        //If any of the categories are set in the Query String then we reset the web part here
        if (((this._urlCategory) && (this._urlCategory != "")) || ((this._urlSubCategory) && (this._urlSubCategory != "")) || ((this._urlPlaylist) && (this._urlPlaylist != ""))) {
          if ((this._urlCategory) && (this._urlCategory != "")) {
            this._startType = Templates.Category;
            this._startLocation = this._urlCategory;
          } else if ((this._urlSubCategory) && (this._urlSubCategory != "")) {
            this._startType = Templates.SubCategory;
            this._startLocation = this._urlSubCategory;
          } else if ((this._urlPlaylist) && (this._urlPlaylist != "")) {
            this._startType = Templates.Playlist;
            this._startLocation = this._urlPlaylist;
            this._startAsset = this._startAsset;
          }else{
            this._startType = "";
            this._startLocation = "";
          }
        }
      }

      //Render web part
      element = React.createElement(
        CustomLearning,
        {
          userSecurity: this._userSecurity,
          config: this._customConfig,
          webpartMode: this.properties.webpartMode,
          startType: this._startType,
          startLocation: this._startLocation,
          startAsset: this._startAsset,
          baseAdminUrl: this._baseAdminUrl,
          baseViewerUrl: this._baseViewerUrl,
          appPartPage: this._appPartPage,
          webpartTitle: this.properties.title
        }
      );
    } else {
      element = React.createElement(
        Error,
        {
          message: `The ${this.LOG_SOURCE} has not been properly configured and you do not have Owner rights to complete the operation. Please contact the site administrator.`
        }
      );
    }
    ReactDom.render(element, this.domElement);
    return;
  }

  protected onDispose(): void {
    ReactDom.unmountComponentAtNode(this.domElement);
  }

  protected get dataVersion(): Version {
    return Version.parse("1.0");
  }

  private getSecurityTrimmedCategories(categories: ICategory[]): ICategory[] {
    var retVal: ICategory[] = [];
    for (let i = 0; i < categories.length; i++) {
      if (categories[i].SubCategories.length > 0) {
        categories[i].SubCategories = this.getSecurityTrimmedCategories(categories[i].SubCategories);
        retVal.push(categories[i]);
      } else {
        if (this.saveCategory(categories[i])) {
          retVal.push(categories[i]);
        }
      }
    }
    return retVal;
  }

  private saveCategory(category: ICategory): boolean {
    let retVal: boolean = true;
    switch (category.Security) {
      case Roles.Owners:
        retVal = (this._userSecurity == Roles.Owners);
        break;
      case Roles.Members:
        retVal = ((this._userSecurity == Roles.Owners) || (this._userSecurity == Roles.Members));
        break;
      case Roles.Visitors:
        retVal = ((this._userSecurity == Roles.Owners) || (this._userSecurity == Roles.Members) || (this._userSecurity == Roles.Visitors));
        break;
      default:
        retVal = true;
    }
    return retVal;
  }

  protected get disableReactivePropertyChanges(): boolean {
    return true; 
  }

  protected onPropertyPaneConfigurationStart(): void {
    if (this._ppCategory && this._ppSubCategory && this._ppPlaylist) {
      return;
    }

    this.getWebpartModePropertyPaneOptions();
    this.getCategoryPropertyPaneOptions();
    this.getSubCategoryPropertyPaneOptions();
    this.getPlaylistPropertyPaneOptions();
    this.getDefaultFilterPropertyPaneOptions();
    this.context.propertyPane.refresh();
  }

  private getWebpartModePropertyPaneOptions(): void {
    let options: IPropertyPaneDropdownOption[] = [];
    options.push({ key: WebpartMode.full, text: WebpartMode.full });
    options.push({ key: WebpartMode.contentonly, text: WebpartMode.contentonly });
    this._ppWebpartMode = options;
  }

  private getDefaultFilterPropertyPaneOptions(): void {
    let options: IPropertyPaneDropdownOption[] = [];
    options.push({ key: "", text: "(none)" });
    options.push({ key: PropertyPaneFilters.category, text: PropertyPaneFilters.category });
    options.push({ key: PropertyPaneFilters.subcategory, text: PropertyPaneFilters.subcategory });
    options.push({ key: PropertyPaneFilters.playlist, text: PropertyPaneFilters.playlist });
    this._ppFilters = options;
  }

  private getCategoryPropertyPaneOptions(): void {
    let options: IPropertyPaneDropdownOption[] = [];
    options.push({ key: "", text: "                                      " });
    for (let i = 0; i < this._customConfig.CachedMetadata.Categories.length; i++) {
      options.push({
        key: this._customConfig.CachedMetadata.Categories[i].Id,
        text: this._customConfig.CachedMetadata.Categories[i].Name,
      });
    }
    options = lodash.sortBy(options, ["text"]);
    this._ppCategory = options;
  }

  private getSubCategoryPropertyPaneOptions(): void {
    //Flatten the nested subcategory list
    let flatSubCat: ISubCategory[] = [];
    for (let i = 0; i < this._customConfig.CachedMetadata.Categories.length; i++) {
      let scFlat = lodash.flatten(this._customConfig.CachedMetadata.Categories[i].SubCategories, true);
      flatSubCat = flatSubCat.concat(scFlat);
    }
    //Build an options list
    let options: IPropertyPaneDropdownOption[] = [];
    options.push({ key: "", text: "                                      " });
    for (let j = 0; j < flatSubCat.length; j++) {
      options.push({
        key: flatSubCat[j].Id,
        text: flatSubCat[j].Name,
      });
    }
    options = lodash.sortBy(options, (o) => {return o.text.toLowerCase();});
    this._ppSubCategory = options;
  }

  private getPlaylistPropertyPaneOptions(): void {
    let options: IPropertyPaneDropdownOption[] = [];
    options.push({ key: "", text: "                                      " });
    for (let i = 0; i < this._customConfig.CachedPlaylists.length; i++) {
      options.push({
        key: this._customConfig.CachedPlaylists[i].Id,
        text: this._customConfig.CachedPlaylists[i].Title,
      });
    }
    options = lodash.sortBy(options, (o) => {return o.text.toLowerCase();});
    this._ppPlaylist = options;
  }

  public loadPlayListAssets = (templateId: string): void => {
    let detail: ICategory[] | ISubCategory[] | IPlaylist[] | IPlaylist;
    detail = lodash.find(this._customConfig.CachedPlaylists, { Id: templateId });
    if(!detail) { return null;}
    let options: IPropertyPaneDropdownOption[] = [];
    for (let i = 0; i < (detail as IPlaylist).Assets.length; i++) {
      let a = lodash.find(this._customConfig.CachedAssets, { Id: (detail as IPlaylist).Assets[i] });
      if (a)
        options.push({
          key: a.Id,
          text: a.Title,
        });
    }
    this._ppAssets = options;
  }

  protected getPropertyPaneConfiguration(): IPropertyPaneConfiguration {
    let displayFilter: any;
    let assetList: any;
    let defaultFilter: any;
    defaultFilter = PropertyFieldDropdownWithCallout('defaultFilter', {
      calloutTrigger: CalloutTriggers.Hover,
      key: 'defaultFilter',
      label: strings.DefaultFilterLabel,
      options: this._ppFilters,
      selectedKey: this.properties.defaultFilter,
      calloutContent: strings.WebpartModeDescription
    });
    assetList = PropertyPaneLabel('defaultAsset', {text: ""});    
    
    switch(this.properties.defaultFilter) {
      case PropertyPaneFilters.category:
        displayFilter = PropertyFieldDropdownWithCallout('defaultCategory', {
          calloutTrigger: CalloutTriggers.Hover,
          key: 'defaultCategory',
          label: strings.DefaultCategoryLabel,
          options: this._ppCategory,
          selectedKey: this.properties.defaultCategory,
          calloutContent: strings.DefaultFilterDescription
        });
        break;
      case PropertyPaneFilters.subcategory:
        displayFilter = PropertyFieldDropdownWithCallout('defaultSubCategory', {
          calloutTrigger: CalloutTriggers.Hover,
          key: 'defaultSubCategory',
          label: strings.DefaultSubCategoryLabel,
          options: this._ppSubCategory,
          selectedKey: this.properties.defaultSubCategory,
          calloutContent: strings.DefaultSubCategoryDescription
        });
        break;
      case PropertyPaneFilters.playlist:
        if (this.properties.defaultPlaylist && this.properties.defaultPlaylist != ""){
          this.loadPlayListAssets(this.properties.defaultPlaylist);
        }
        displayFilter = PropertyFieldDropdownWithCallout('defaultPlaylist', {
          calloutTrigger: CalloutTriggers.Hover,
          key: 'defaultPlaylist',
          label: strings.DefaultPlaylistLabel,
          options: this._ppPlaylist,
          selectedKey: this.properties.defaultPlaylist,
          calloutContent: strings.DefaultPlaylistDescription
        });
        assetList = PropertyFieldDropdownWithCallout('defaultAsset', {
          calloutTrigger: CalloutTriggers.Hover,
          key: 'defaultAsset',
          label: strings.DefaultAssetLabel,
          options: this._ppAssets,
          selectedKey: this.properties.defaultAsset,
          calloutContent: strings.DefaultAssetDescription
        });
        break;
      default:
        displayFilter = PropertyFieldDropdownWithCallout('defaultCategory', {
          calloutTrigger: CalloutTriggers.Hover,
          key: 'defaultCategory',
          label: strings.DefaultCategoryLabel,
          options: this._ppCategory,
          selectedKey: this.properties.defaultCategory,
          calloutContent: strings.DefaultFilterDescription
        });
    }

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
                PropertyPaneTextField('title', {
                  label: strings.WebpartTitleLabel,
                }),
                PropertyFieldDropdownWithCallout('webpartMode', {
                  calloutTrigger: CalloutTriggers.Hover,
                  key: 'webpartMode',
                  label: strings.WebpartModeLabel,
                  options: this._ppWebpartMode,
                  selectedKey: this.properties.webpartMode,
                  calloutContent: strings.WebpartModeDescription
                }),
                defaultFilter,
                displayFilter,
                assetList                
              ]
            }
          ]
        }
      ]
    };
  }

  protected onPropertyPaneFieldChanged(propertyPath: string, oldValue: any, newValue: any): void {
    //The default filter drop down changed
    if (propertyPath === 'defaultFilter' && newValue) {
      super.onPropertyPaneFieldChanged(propertyPath, oldValue, newValue);
      this.properties.defaultCategory = "";
      this.properties.defaultSubCategory = "";
      this.properties.defaultPlaylist = "";
      this.properties.defaultAsset = "";
      //this.render();
      this.context.propertyPane.refresh();
    }else if (propertyPath === 'defaultPlaylist'){
      this.loadPlayListAssets(newValue);
    }
    else {
      super.onPropertyPaneFieldChanged(propertyPath, oldValue, newValue);
    }
  }
}
