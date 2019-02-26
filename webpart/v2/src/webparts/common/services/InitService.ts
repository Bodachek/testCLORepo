import { Logger, LogLevel } from "@pnp/logging";
import * as lodash from 'lodash';
import { sp, Web } from '@pnp/sp';
import "@pnp/polyfill-ie11";
import { Environment, EnvironmentType } from '@microsoft/sp-core-library';
import { ConfigService } from "./ConfigService";
import { Roles } from "../models/Enums";

export interface IInitService {
  LearningSite: string;
  TelemetryOn: boolean;
  initialize(): Promise<void>;
  validateLists(owner: boolean): Promise<boolean>;
  getUser(): Promise<string>;
}

export class InitService implements IInitService {
  private LOG_SOURCE = "InitService";
  
  public LearningSite: string;
  public TelemetryOn: boolean = true;

  constructor() {  
  } 

  public async initialize(): Promise<void>{
    try {
      await this.loadTelemetryOn();
      await this.loadLearningSite();
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (initialize)`, LogLevel.Error);
    }
  }

  //Read value of MicrosoftCustomLearningSite from Tenant Properties
  private async loadTelemetryOn(): Promise<void> {
    try {
      let telemetryOn = await sp.web.getStorageEntity("MicrosoftCustomLearningTelemetryOn");
      if (telemetryOn.Value) {
        this.TelemetryOn = (telemetryOn.Value == "true");
      } else {
        Logger.write(`${this.LOG_SOURCE} (setLearningSite) -- Tenant property 'MicrosoftCustomLearningTelemetryOn' has not been set.`, LogLevel.Error);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (loadLearningSite)`, LogLevel.Error);
    }
    return;
  }

  //Read value of MicrosoftCustomLearningSite from Tenant Properties
  private async loadLearningSite(): Promise<void> {
    try {
      let learningSite = await sp.web.getStorageEntity("MicrosoftCustomLearningSite");
      if (learningSite.Value) {
        if(learningSite.Value.startsWith("http")){
          this.LearningSite = learningSite.Value;
        }else{
          this.LearningSite = `${document.location.origin}${learningSite.Value}`;
        }
      } else {
        Logger.write(`${this.LOG_SOURCE} (setLearningSite) -- Tenant property 'MicrosoftCustomLearningSite' has not been set.`, LogLevel.Error);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (loadLearningSite)`, LogLevel.Error);
    }
    return;
  }

  //Validate that custom playlist and assets SharePoint lists exist based on web part properties
  public async validateLists(owner: boolean): Promise<boolean> {
    try {
      let configService = new ConfigService(new Web(this.LearningSite));

      let listsCheck: Promise<boolean>[] = [];
      listsCheck.push(configService.validateConfig(owner));
      listsCheck.push(configService.validatePlaylists(owner));
      listsCheck.push(configService.validateAssets(owner));

      let validateResults = await Promise.all(listsCheck);

      for (let i = 0; i < validateResults.length; i++) {
        if (!validateResults[i]) {
          return false;
        }
      }
      return true;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (validateLists)`, LogLevel.Error);
      return false;
    }
  }

  //Calls SharePoint REST Api to determine if user is in Owners, Members, or Visitors group
  //Used to filter categories in metadata.json by the Security property
  public async getUser(): Promise<string> {
    try {
      if (DEBUG && Environment.type === EnvironmentType.Local) {
        // If the running environment is local, load the data from the mock
        return Roles.Owners;
      } else {
        let data = await sp.web.currentUser.expand("groups").get<{ IsSiteAdmin: boolean, Groups: { Title: string }[] }>();
        let ownerIndex: number = lodash.findIndex(data.Groups, o => (o["Title"].indexOf(Roles.Owners) > -1));
        if(data.IsSiteAdmin){
          ownerIndex = 0;
        }
        let membersIndex: number = lodash.findIndex(data.Groups, o => (o["Title"].indexOf(Roles.Members) > -1));
        if (ownerIndex > -1)
          return Roles.Owners;
        else if (membersIndex > -1)
          return Roles.Members;
        else
          return Roles.Visitors;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getUser)`, LogLevel.Error);
      return Roles.Visitors;
    }
  }
}