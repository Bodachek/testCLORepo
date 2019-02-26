
import { Logger, LogLevel } from "@pnp/logging";
import { Web } from '@pnp/sp';
import { IPlaylist, IAsset, ICustomConfig, ICategory, ICustomSubCategories } from '../models/Models';
import "@pnp/polyfill-ie11";
import { CustomListNames } from "../models/Enums";

export interface ICustomDataService {
  createPlaylist(newPlaylist: IPlaylist): Promise<number>;
  modifyPlaylist(editPlaylist: IPlaylist): Promise<string>;
  deletePlaylist(playlistId: string): Promise<boolean>;
  createAsset(newAsset: IAsset): Promise<number>;
  modifyAsset(editAsset: IAsset): Promise<string>;
  createConfig(newConfig: ICustomConfig): Promise<number>;
  modifyConfig(editConfig: ICustomConfig): Promise<string>;
  createSubCategories(newCategory: ICustomSubCategories): Promise<number>;
  modifySubCategories(editCategory: ICustomSubCategories): Promise<string>;
}

export class CustomDataService implements ICustomDataService {
  private LOG_SOURCE = "CustomDataService";
  private _web: Web;

  constructor(learningWeb: Web) {
    this._web = learningWeb;
  }

  //Creates a custom playlist stored in local SharePoint list (this code assumes the same site collection)
  public async createPlaylist(newPlaylist: IPlaylist): Promise<number> {
    try {
      delete newPlaylist['@odata.etag'];
      let item = {Title: newPlaylist.Title, JSONData: JSON.stringify(newPlaylist)};
      let newPlaylistResponse = await this._web.lists.getByTitle(CustomListNames.customPlaylistsName).items.add(item);

      return newPlaylistResponse.data.Id;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (createPlaylist)`, LogLevel.Error);
      return 0;
    }
  }

  //Updates a custom playlist stored in local SharePoint list (this code assumes the same site collection)
  public async modifyPlaylist(editPlaylist: IPlaylist): Promise<string> {
    try {
      delete editPlaylist['@odata.etag'];
      let item = {JSONData: JSON.stringify(editPlaylist)};
      let updatedPlaylistResponse = await this._web.lists.getByTitle(CustomListNames.customPlaylistsName).items.getById(+editPlaylist.Id).update(item);

      return updatedPlaylistResponse.data["odata.etag"].split('\"')[1].toString();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (modifyPlaylist)`, LogLevel.Error);
      return "0";
    }
  }

  //Deletes a custom playlist stored in local SharePoint list (this code assumes the same site collection)
  //Does not remove associated assets, could be updated to look for orphaned assets and act accordingly
  public async deletePlaylist(playlistId: string): Promise<boolean> {
    try {
      await this._web.lists.getByTitle(CustomListNames.customPlaylistsName).items.getById(+playlistId).delete();

      return true;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (deletePlaylist)`, LogLevel.Error);
      return false;
    }
  }

  //Creates a custom playlist asset stored in local SharePoint list (this code assumes the same site collection)
  public async createAsset(newAsset: IAsset): Promise<number> {
    try {
      delete newAsset['@odata.etag'];      
      let item = {Title: newAsset.Title, JSONData: JSON.stringify(newAsset)};
      let newAssetResponse = await this._web.lists.getByTitle(CustomListNames.customAssetsName).items.add(item);

      return newAssetResponse.data.Id;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (createAsset)`, LogLevel.Error);
      return 0;
    }
  }

  //Updates a custom playlist asset stored in local SharePoint list (this code assumes the same site collection)
  public async modifyAsset(editAsset: IAsset): Promise<string> {
    try {
      delete editAsset['@odata.etag'];
      let item = {JSONData: JSON.stringify(editAsset)};
      let updatedAssetResponse = await this._web.lists.getByTitle(CustomListNames.customAssetsName).items.getById(+editAsset.Id).update(item);

      return updatedAssetResponse.data["odata.etag"].split('\"')[1].toString();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (modifyAsset)`, LogLevel.Error);
      return "0";
    }
  }

  //Creates a custom config stored in local SharePoint list
  public async createConfig(newConfig: ICustomConfig): Promise<number> {
    try {
      delete newConfig['@odata.etag'];
      let newConfigResponse = await this._web.lists.getByTitle(CustomListNames.customConfigName).items.add({ Title: "CustomConfig", JSONData: JSON.stringify(newConfig) });

      return newConfigResponse.data.Id;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (createConfig)`, LogLevel.Error);
      return 0;
    }
  }

  //Updates a custom config stored in local SharePoint list 
  public async modifyConfig(editConfig: ICustomConfig): Promise<string> {
    try {
      delete editConfig['@odata.etag'];
      let updatedConfigResponse = await this._web.lists.getByTitle(CustomListNames.customConfigName).items.getById(editConfig.Id).update({ JSONData: JSON.stringify(editConfig) });

      return updatedConfigResponse.data["odata.etag"].split('\"')[1].toString();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (modifyConfig)`, LogLevel.Error);
      return "0";
    }
  }

  //Creates a custom sub category array stored in local SharePoint list;
  public async createSubCategories(newCategory: ICustomSubCategories): Promise<number> {
    try {
      delete newCategory['@odata.etag'];
      let newConfigResponse = await this._web.lists.getByTitle(CustomListNames.customConfigName).items.add({ Title: "CustomSubCategories", JSONData: JSON.stringify(newCategory) });

      return newConfigResponse.data.Id;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (createSubCategories)`, LogLevel.Error);
      return 0;
    }
  }

  //Updates a custom sub category array stored in local SharePoint list 
  public async modifySubCategories(editCategory: ICustomSubCategories): Promise<string> {
    try {
      delete editCategory['@odata.etag'];
      let updatedConfigResponse = await this._web.lists.getByTitle(CustomListNames.customConfigName).items.getById(+editCategory.Id).update({ JSONData: JSON.stringify(editCategory) });

      return updatedConfigResponse.data["odata.etag"].split('\"')[1].toString();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (modifySubCategories)`, LogLevel.Error);
      return "0";
    }
  }
}