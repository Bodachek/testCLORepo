import { Logger, LogLevel } from "@pnp/logging";
import { Web } from "@pnp/sp";
import "@pnp/polyfill-ie11";
import { CustomListNames } from "../models/Enums";

export interface IConfigService {
  validatePlaylists(owner: boolean): Promise<boolean>;
  validateAssets(owner: boolean): Promise<boolean>;
  validateConfig(owner: boolean): Promise<boolean>;
}

export class ConfigService implements IConfigService {
  private LOG_SOURCE = "ConfigService";
  private _learningWeb: Web;

  constructor(learningWeb: Web) {
    this._learningWeb = learningWeb;
  }

  public async validatePlaylists(owner: boolean): Promise<boolean> {
    try {
      let playlistCheck = await this._learningWeb.lists.getByTitle(CustomListNames.customPlaylistsName).fields.select("Title").filter("Title eq 'JSONData'").get<[{ Title: string }]>();
      if (playlistCheck.length !== 1) {
        if (owner) {
          try {
            //List exists, field doesn't
            let playlistFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customPlaylistsName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
          } catch (err) {
            Logger.write(`${err} - ${this.LOG_SOURCE} (validatePlaylists)`, LogLevel.Error);
            return false;
          }
        } else {
          Logger.write(`${this.LOG_SOURCE} (validatePlaylists) -- User does not have appropriate rights to create field in custom playlists list.`, LogLevel.Error);
          return false;
        }
      }
    } catch (err) {
      //Assume list doesn't exist
      if (owner) {
        try {
          let playlistAdd = await this._learningWeb.lists.add(CustomListNames.customPlaylistsName, "Microsoft Custom Learning - Custom Playlists", 100, true);
          let playlistFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customPlaylistsName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
        } catch (err) {
          Logger.write(`${err} - ${this.LOG_SOURCE} (validatePlaylists)`, LogLevel.Error);
          return false;
        }
      } else {
        Logger.write(`${this.LOG_SOURCE} (validatePlaylists) -- User does not have appropriate rights to create custom playlists list.`, LogLevel.Error);
        return false;
      }
    }
    return true;
  }

  public async validateAssets(owner: boolean): Promise<boolean> {
    try {
      let assetsCheck = await this._learningWeb.lists.getByTitle(CustomListNames.customAssetsName).fields.select("Title").filter("Title eq 'JSONData'").get<[{ Title: string }]>();
      if (assetsCheck.length !== 1) {
        if (owner) {
          try {
            //List exists, field doesn't
            let assetsFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customAssetsName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
          } catch (err) {
            Logger.write(`${err} - ${this.LOG_SOURCE} (validateAssets)`, LogLevel.Error);
            return false;
          }
        } else {
          Logger.write(`${this.LOG_SOURCE} (validateAssets) -- User does not have appropriate rights to create field in custom assets list.`, LogLevel.Error);
          return false;
        }
      }
    } catch (err) {
      //Assume list doesn't exist
      if (owner) {
        try {
          let assetsAdd = await this._learningWeb.lists.add(CustomListNames.customAssetsName, "Microsoft Custom Learning - Custom Assets", 100, true);
          let assetFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customAssetsName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
        } catch (err) {
          Logger.write(`${err} - ${this.LOG_SOURCE} (validateAssets)`, LogLevel.Error);
          return false;
        }
      } else {
        Logger.write(`${this.LOG_SOURCE} (validateAssets) -- User does not have appropriate rights to create custom assets list.`, LogLevel.Error);
        return false;
      }
    }
    return true;
  }

  private async getRoleInformation(): Promise<number[]> {
    let retVal: number[] = [];
    const targetRoleName = "Contribute";
    try {
      let targetGroups = await this._learningWeb.siteGroups.filter("substringof('Visitor', Title)").top(1).get();
      if (targetGroups.length == 1) {
        let targetGroupId = targetGroups[0].Id;
        let roleDefinition = await this._learningWeb.roleDefinitions.getByName(targetRoleName).get();
        let roleDefinitionId = roleDefinition.Id;
        retVal.push(targetGroupId);
        retVal.push(roleDefinitionId);
      } else {
        Logger.write(`${this.LOG_SOURCE} (getRoleInformation) - Could not load Visitor group information.`, LogLevel.Error);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (validateAssets)`, LogLevel.Error);
    }
    return retVal;
  }

  public async validateConfig(owner: boolean): Promise<boolean> {
    try {
      let configCheck = await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).fields.select("Title").filter("Title eq 'JSONData'").get<[{ Title: string }]>();
      if (configCheck.length !== 1) {
        if (owner) {
          try {
            //List exists, field doesn't
            let configFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
          } catch (err) {
            Logger.write(`${err} - ${this.LOG_SOURCE} (validateConfig)`, LogLevel.Error);
            return false;
          }
        } else {
          Logger.write(`${this.LOG_SOURCE} (validateConfig) -- User does not have appropriate rights to create field in custom config list.`, LogLevel.Error);
          return false;
        }
      }
    } catch (err) {
      //Assume list doesn't exist
      if (owner) {
        try {
          let configAdd = await this._learningWeb.lists.add(CustomListNames.customConfigName, "Microsoft Custom Learning - Custom Config", 100, true);
          let configFieldAdd = await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).fields.add("JSONData", "SP.Field", { "FieldTypeKind": 3 });
          let configBreakInheritance = await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).breakRoleInheritance(true);
          let configPermissions = await this.getRoleInformation();
          if (configPermissions.length > 0) {
            await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).roleAssignments.getById(configPermissions[0]).delete();
            await this._learningWeb.lists.getByTitle(CustomListNames.customConfigName).roleAssignments.add(configPermissions[0], configPermissions[1]);
          } else {
            Logger.write(`${this.LOG_SOURCE} (validateConfig) - ${CustomListNames.customConfigName} list created but permissions could not be set.`, LogLevel.Error);
            return false;
          }
        } catch (err) {
          Logger.write(`${err} - ${this.LOG_SOURCE} (validateConfig)`, LogLevel.Error);
          return false;
        }
      } else {
        Logger.write(`${this.LOG_SOURCE} (validateConfig) -- User does not have appropriate rights to create custom config list.`, LogLevel.Error);
        return false;
      }
    }
    return true;
  }
}