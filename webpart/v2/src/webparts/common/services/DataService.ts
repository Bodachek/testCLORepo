import { Logger, LogLevel } from "@pnp/logging";
import * as lodash from "lodash";
import { sp, Web } from "@pnp/sp";
import "@pnp/polyfill-ie11";
import { ICustomConfig, CustomConfig, IPlaylist, IAsset, IMetadata, ITechnology, ICustomSubCategories, CustomSubCategories } from "../models/Models";
import { CustomListNames } from "../models/Enums";
import { Environment, EnvironmentType } from "@microsoft/sp-core-library";
import { HttpClientResponse, HttpClient } from "@microsoft/sp-http";
import { CustomDataService, ICustomDataService } from "./CustomDataService";

export interface IDataService {
  learningWeb: Web;
  cdnBase: string;
  metadata: IMetadata;
  customSubCategories: ICustomSubCategories;
  playlistsAll: IPlaylist[];
  assetsAll: IAsset[];
  getCustomConfig(): Promise<ICustomConfig>;
  getMetadata(): Promise<IMetadata>;
  getPlaylists(): Promise<IPlaylist[]>;
  refreshConfig(config: ICustomConfig): Promise<ICustomConfig>;
  refreshConfigCustomOnly(config: ICustomConfig): Promise<ICustomConfig>;
  refreshPlaylistsAll(customOnly: boolean): Promise<IPlaylist[]>;
  refreshAssetsAll(customOnly: boolean): Promise<IAsset[]>;
}

export class DataService implements IDataService {
  public cdnBase: string;
  private LOG_SOURCE = "DataService";
  public learningWeb: Web;
  public metadata: IMetadata;
  public customSubCategories: ICustomSubCategories;
  public playlistsAll: IPlaylist[];
  public assetsAll: IAsset[];

  private _ready: boolean = false;
  private _httpClient: HttpClient;
  private _downloadedPlaylists: IPlaylist[];
  private _downloadedAssets: IAsset[];
  private _customDataService: ICustomDataService;
  private _loadCdnBaseTry: number = 0;

  private fieldOptions = {
    headers: { Accept: "application/json;odata.metadata=none" }
  };

  constructor(httpClient: HttpClient, learningSite: string) {
    try {
      this._httpClient = httpClient;
      this.learningWeb = new Web(learningSite);
      this.loadCdnBase();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (constructor)`, LogLevel.Error);
    }
  }

  private async delay(ms: number): Promise<any> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private async isReady(): Promise<boolean> {
    let startTime = new Date().getTime();
    while (!this._ready || new Date().getTime() - startTime > 60000) {
      await this.delay(200);
    }
    return this._ready;
  }

  //Read value of MicrosoftCustomLearningCdn from Tenant Properties
  private async loadCdnBase(): Promise<void> {
    try {
      let cdn = await sp.web.getStorageEntity("MicrosoftCustomLearningCdn");
      if (cdn.Value) {
        this.cdnBase = cdn.Value;
        if (this.cdnBase) {
          this._ready = true;
        }
        else {
          this._loadCdnBaseTry++;
          if (this._loadCdnBaseTry < 5)
            this.loadCdnBase();
        }
      } else {
        Logger.write(`${this.LOG_SOURCE} (setCdnBase) -- Tenant property 'MicrosoftCustomLearningCdn' has not been set.`, LogLevel.Error);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (loadCdnBase)`, LogLevel.Error);
    }
    return;
  }

  //Optional formatter for dates using JSON.parse
  private dateTimeReviver(key, value) {
    const dateFormat: RegExp = /^\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d{3}Z$/;
    if (typeof value === "string" && dateFormat.test(value)) {
      return new Date(value);
    }

    return value;
  }

  //Gets custom configuration stored in local SharePoint list
  public async getCustomConfig(): Promise<ICustomConfig> {
    let config: ICustomConfig = new CustomConfig();

    if (DEBUG && Environment.type === EnvironmentType.Local) {
      // If the running environment is local, load the data from the mock
      return (require('../../../../mocks/config.json'));
    } else {
      try {
        let configResponse = await this.learningWeb.lists.getByTitle(CustomListNames.customConfigName).items.select("Id", "Title", "JSONData").orderBy("Id", false).filter("Title eq 'CustomConfig'").top(1).get<[{ Id: string, Title: string, JSONData: string }]>();
        if (configResponse.length == 1) {
          if (configResponse[0].JSONData.length > 0) {
            config = JSON.parse(configResponse[0].JSONData, this.dateTimeReviver);
            config.Id = +configResponse[0].Id;
            config.eTag = JSON.parse(configResponse[0]["odata.etag"]);
          }
        } else {
          config = null;
        }
      } catch (err) {
        config = null;
        Logger.write(`${err} - ${this.LOG_SOURCE} (getCustomConfig)`, LogLevel.Error);
      }
    }

    return config;
  }

  //Gets custom configuration stored in local SharePoint list
  public async getCustomSubCategories(): Promise<ICustomSubCategories> {
    let config: ICustomSubCategories = new CustomSubCategories();

    if (DEBUG && Environment.type === EnvironmentType.Local) {
      // If the running environment is local, load the data from the mock
      return (require('../../../../mocks/config.json'));
    } else {
      try {
        let configResponse = await this.learningWeb.lists.getByTitle(CustomListNames.customConfigName).items.select("Id", "Title", "JSONData").filter("Title eq 'CustomSubCategories'").top(1).get<[{ Id: string, Title: string, JSONData: string }]>();
        if (configResponse.length == 1) {
          if (configResponse[0].JSONData.length > 0) {
            config = JSON.parse(configResponse[0].JSONData);
            config.Id = configResponse[0].Id;
            config.eTag = JSON.parse(configResponse[0]["odata.etag"]);
          }
        } else {
          config = new CustomSubCategories();
        }
      } catch (err) {
        config = null;
        Logger.write(`${err} - ${this.LOG_SOURCE} (getCustomSubCategories)`, LogLevel.Error);
      }
    }

    return config;
  }

  //Gets custom playlists stored in local SharePoint list (this code assumes the same site collection)
  private async getCustomPlaylists(): Promise<IPlaylist[]> {
    if (DEBUG && Environment.type === EnvironmentType.Local) {
      // If the running environment is local, load the data from the mock
      return (require('../../../../mocks/customPlaylists.json'));
    } else {
      let customPlaylists: IPlaylist[] = [];
      let playlists = await this.learningWeb.lists.getByTitle(CustomListNames.customPlaylistsName).items.select("Id", "Title", "JSONData").get<[{ Id: string, Title: string, JSONData: string }]>();
      for (let i = 0; i < playlists.length; i++) {
        try {
          let playlist: IPlaylist = JSON.parse(playlists[i].JSONData);
          playlist["@odata.etag"] = playlists[i]["@odata.etag"];
          playlist.Id = `${playlists[i].Id}`;
          customPlaylists.push(playlist);
        } catch (err) {
          Logger.write(`${err} - ${this.LOG_SOURCE} (getCustomPlaylists)`, LogLevel.Error);
        }
      }

      return customPlaylists;
    }
  }

  //Gets custom playlist assets stored in local SharePoint list (this code assumes the same site collection)
  private async getCustomAssets(): Promise<IAsset[]> {
    if (DEBUG && Environment.type === EnvironmentType.Local) {
      // If the running environment is local, load the data from the mock
      return (require('../../../../mocks/customAssets.json'));
    } else {
      let customAssets: IAsset[] = [];
      let assets = await this.learningWeb.lists.getByTitle(CustomListNames.customAssetsName).items.select("Id", "Title", "JSONData").get<[{ Id: string, Title: string, JSONData: string }]>();
      for (let i = 0; i < assets.length; i++) {
        try {
          let asset: IAsset = JSON.parse(assets[i].JSONData);
          asset["@odata.etag"] = assets[i]["@odata.etag"];
          asset.Id = `${assets[i].Id}`;
          customAssets.push(asset);
        } catch (err) {
          Logger.write(`${err} - ${this.LOG_SOURCE} (getCustomAssets)`, LogLevel.Error);
        }
      }
      return customAssets;
    }
  }

  //Loads Metadata.json file from Microsoft CDN
  public async getMetadata(): Promise<IMetadata> {
    try {
      if (DEBUG && Environment.type === EnvironmentType.Local) {
        // If the running environment is local, load the data from the mock
        let metadata: IMetadata = require("../../../../../../docs/v2/metadata.json");
        return metadata;
      } else {
        let ready = await this.isReady();
        if (ready) {
          let results: HttpClientResponse = await this._httpClient.fetch(`${this.cdnBase}metadata.json`, HttpClient.configurations.v1, this.fieldOptions);
          if (results.ok) {
            let resultsJson: IMetadata = await results.json();
            for (let t = 0; t < resultsJson.Technologies.length; t++) {
              if (resultsJson.Technologies[t].Image.length > 1)
                resultsJson.Technologies[t].Image = `${this.cdnBase}${resultsJson.Technologies[t].Image}`;
            }
            for (let c = 0; c < resultsJson.Categories.length; c++) {
              for (let sc = 0; sc < resultsJson.Categories[c].SubCategories.length; sc++) {
                if (resultsJson.Categories[c].SubCategories[sc].Image.length > 1)
                  resultsJson.Categories[c].SubCategories[sc].Image = `${this.cdnBase}${resultsJson.Categories[c].SubCategories[sc].Image}`;
              }
            }
            for (let a = 0; a < resultsJson.Audiences.length; a++) {
              if (resultsJson.Audiences[a].Image.length > 1)
                resultsJson.Audiences[a].Image = `${this.cdnBase}${resultsJson.Audiences[a].Image}`;
            }
            return resultsJson;
          } else {
            Logger.write(`${this.LOG_SOURCE} (getMetadata) Fetch Error: ${results.statusText}`, LogLevel.Error);
            return null;
          }
        } else {
          Logger.write(`Webpart Not Ready - ${this.LOG_SOURCE} (getMetadata)`, LogLevel.Error);
        }
        return null;
      }
    }
    catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getMetadata)`, LogLevel.Error);
      return null;
    }
  }

  //Loads playlists.json file from Microsoft CDN
  public async getPlaylists(): Promise<IPlaylist[]> {
    try {
      if (DEBUG && Environment.type === EnvironmentType.Local) {
        // If the running environment is local, load the data from the mock
        return require("../../../../../../docs/v2/playlists.json");
      } else {
        let ready = await this.isReady;
        if (ready) {
          let results: HttpClientResponse = await this._httpClient.fetch(`${this.cdnBase}playlists.json`, HttpClient.configurations.v1, this.fieldOptions);
          if (results.ok) {
            let resultsJson: IPlaylist[] = await results.json();
            for (let i = 0; i < resultsJson.length; i++) {
              if (resultsJson[i].Image.length > 1)
                resultsJson[i].Image = `${this.cdnBase}${resultsJson[i].Image}`;
            }
            return resultsJson;
          } else {
            Logger.write(`${this.LOG_SOURCE} (getPlaylists) Fetch Error: ${results.statusText}`, LogLevel.Error);
            return null;
          }
        } else {
          return null;
        }
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getPlaylists)`, LogLevel.Error);
      return null;
    }
  }

  //Loads assets.json file from Microsoft CDN
  private async getAssets(): Promise<IAsset[]> {
    try {
      if (DEBUG && Environment.type === EnvironmentType.Local) {
        // If the running environment is local, load the data from the mock
        return require("../../../../../../docs/v2/assets.json");
      } else {
        let ready = await this.isReady;
        if (ready) {
          let results: HttpClientResponse = await this._httpClient.fetch(`${this.cdnBase}assets.json`, HttpClient.configurations.v1, this.fieldOptions);
          if (results.ok) {
            let resultsJson: IAsset[] = await results.json();
            return resultsJson;
          } else {
            return null;
          }
        } else {
          return null;
        }
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getAssets)`, LogLevel.Error);
      return null;
    }
  }

  public async refreshPlaylistsAll(customOnly: boolean = false): Promise<IPlaylist[]> {
    let playlists = lodash.cloneDeep(this._downloadedPlaylists);
    try {
      if (!customOnly || this._downloadedPlaylists.length < 1) {
        playlists = await this.getPlaylists();
        this._downloadedPlaylists = lodash.cloneDeep(playlists);
      }
      let customPlaylists = await this.getCustomPlaylists();
      if (customPlaylists) {
        playlists = playlists.concat(customPlaylists);
      }
      this.playlistsAll = lodash.cloneDeep(playlists);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (refreshPlaylistsAll)`, LogLevel.Error);
    }

    return playlists;
  }

  public async refreshAssetsAll(customOnly: boolean = false): Promise<IAsset[]> {
    let assets = lodash.cloneDeep(this._downloadedAssets);
    try {
      if (!customOnly || this._downloadedAssets.length < 1) {
        assets = await this.getAssets();
        this._downloadedAssets = lodash.cloneDeep(assets);
      }
      let customAssets = await this.getCustomAssets();
      if (customAssets) {
        assets = assets.concat(customAssets);
      }
      this.assetsAll = lodash.cloneDeep(assets);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (refreshAssetsAll)`, LogLevel.Error);
    }
    return assets;
  }

  public filterPlaylists(playlists: IPlaylist[], hiddenPlaylistsIds: string[], technologies: ITechnology[]): IPlaylist[] {
    try {
      //Merge customer hidden playlists
      if (hiddenPlaylistsIds.length > 0) {
        playlists = lodash.remove(playlists, (item) => {
          return !lodash.includes(hiddenPlaylistsIds, item.Id);
        });
      }

      //Remove Playlists where Technologies are hidden
      if (technologies && technologies.length > 0) {
        let filteredPlaylists = [];
        for (let i = 0; i < technologies.length; i++) {
          let pl = lodash.filter(playlists, { Technology: technologies[i].Name });
          if (pl && technologies[i].Subjects.length > 0) {
            //validate subject
            pl = lodash.filter(pl, (item) => {
              if (item.Subject === "") return true;
              return (technologies[i].Subjects.indexOf(item.Subject) > -1);
            });
          }
          if (pl && pl.length > 0) {
            filteredPlaylists = filteredPlaylists.concat(pl);
          }
        }
        if (filteredPlaylists.length > 0)
          playlists = filteredPlaylists;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (filterPlaylists)`, LogLevel.Error);
    }
    return playlists;
  }

  //Gets cdn config and merges with custom configuration and updates cache
  public async refreshConfig(config: ICustomConfig): Promise<ICustomConfig> {
    try {
      let metadata = lodash.cloneDeep(await this.getMetadata());
      this.customSubCategories = await this.getCustomSubCategories();

      //Merge custom created subcategories
      if (this.customSubCategories.CustomSubcategories.length > 0) {
        this.customSubCategories.CustomSubcategories.forEach((catItem) => {
          if (catItem.SubCategories.length > 0) {
            let cat = lodash.find(metadata.Categories, { Id: catItem.Id });
            cat.SubCategories = cat.SubCategories.concat(catItem.SubCategories);
          }
        });
      }

      //Make a copy for public property (includes custom sub categories)
      this.metadata = lodash.cloneDeep(metadata);

      //Merge customer hidden technology
      if (config.HiddenTechnology.length > 0 || config.HiddenSubject.length > 0) {
        metadata.Technologies = lodash.remove(metadata.Technologies, (item) => {
          if (item.Subjects.length > 0) {
            item.Subjects = lodash.remove(item.Subjects, (subject) => {
              return (config.HiddenSubject.indexOf(subject) < 0);
            });
          }
          return (config.HiddenTechnology.indexOf(item.Name) < 0);
        });
      }

      //Merge customer hidden subcategories
      if (config.HiddenSubCategories.length > 0) {
        metadata.Categories = lodash.remove(metadata.Categories, (item) => {
          if (item.SubCategories.length > 0) {
            item.SubCategories = lodash.remove(item.SubCategories, (sub) => {
              return (config.HiddenSubCategories.indexOf(sub.Id) < 0);
            });
          }
          return (item.SubCategories.length > 0);
        });
      }

      //Get playlists and custom playlists
      let playlists = await this.refreshPlaylistsAll();
      //Get assets and custom assets
      let assets = await this.refreshAssetsAll();

      //Filter playlists for cache
      playlists = this.filterPlaylists(playlists, config.HiddenPlaylistsIds, metadata.Technologies);

      for (let countC = 0; countC < metadata.Categories.length; countC++) {
        for (let sc = 0; sc < metadata.Categories[countC].SubCategories.length; sc++) {
          let selectedPlaylist = lodash.countBy(playlists, { 'CatId': metadata.Categories[countC].SubCategories[sc].Id });
          metadata.Categories[countC].SubCategories[sc].Count = selectedPlaylist.true;
        }
      }

      //Update config cache
      config.CachedMetadata = metadata;
      config.CachedPlaylists = playlists;
      config.CachedAssets = assets;
      config.LastUpdated = new Date();

      //Save config to list
      let c = lodash.cloneDeep(config);
      let customData = this._customDataService || new CustomDataService(this.learningWeb);
      if (config.Id > 0) {
        let updateConfig = await customData.modifyConfig(c);
        config.eTag = updateConfig;
      } else {
        //Create first config
        let addConfig = await customData.createConfig(c);
        config.Id = addConfig;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (refreshConfig)`, LogLevel.Error);
    }

    return config;
  }

  public async refreshConfigCustomOnly(config: ICustomConfig): Promise<ICustomConfig> {
    try {
      let playlists = lodash.cloneDeep(this.playlistsAll);
      //Filter playlists for cache
      this.filterPlaylists(playlists, config.HiddenPlaylistsIds, config.CachedMetadata.Technologies);
      config.CachedPlaylists = playlists;
      config.CachedAssets = this.assetsAll;
      config.LastUpdated = new Date();
      let c = lodash.cloneDeep(config);
      let customData = this._customDataService || new CustomDataService(this.learningWeb);
      let updateConfig = await customData.modifyConfig(c);
      config.eTag = updateConfig;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (refreshConfigCustomOnly)`, LogLevel.Error);
    }
    return config;
  }
}