import { Logger, LogLevel } from "@pnp/logging";
import { InitService } from "./InitService";
import { Roles } from "../models/Enums";
import { ICustomConfig, CustomConfig } from "../models/Models";
import { DataService } from "./DataService";
import { HttpClient } from "@microsoft/sp-http";

export interface ICacheController {
  IsReady: () => Promise<boolean>;
  IsValid: boolean;
  LearningSite: string;
  TelemetryOn: boolean;
  UserSecurity: string;
  CustomConfig: ICustomConfig;
}

export default class CacheController implements ICacheController {
  private LOG_SOURCE = "CacheController";
  public IsValid: boolean = true;
  public LearningSite: string;
  public TelemetryOn: boolean = true;
  public UserSecurity: string = "";
  public CustomConfig: ICustomConfig;

  private _ready: boolean = false;
  private static _instance: CacheController;
  private _httpClient: HttpClient;

  public constructor(httpClient: HttpClient) {
    this._httpClient = httpClient;
    this.doInit();
  }

  public static getInstance(httpClient: HttpClient): CacheController {
    if (!CacheController._instance) {
      CacheController._instance = new CacheController(httpClient);
    }
    return CacheController._instance;
  }

  private async delay(ms: number): Promise<any> {
    return new Promise(resolve => setTimeout(resolve, ms));
  }

  private async doInit(): Promise<void> {
    try {
      let initService = new InitService();
      await initService.initialize();
      this.UserSecurity = await initService.getUser();
      this.TelemetryOn = initService.TelemetryOn;
      this.LearningSite = initService.LearningSite;
      let dataService = new DataService(this._httpClient, this.LearningSite);
      this.CustomConfig = await dataService.getCustomConfig();
      if (!this.CustomConfig) {
        this.IsValid = await initService.validateLists((this.UserSecurity === Roles.Owners));
        if (this.IsValid) {
          this.CustomConfig = new CustomConfig();
          //Test if cache is out of date
          let yesterday: Date = new Date();
          yesterday.setDate(yesterday.getDate() + -1);
          if (!this.CustomConfig.LastUpdated || this.CustomConfig.LastUpdated < yesterday) {
            this.CustomConfig = await dataService.refreshConfig(this.CustomConfig);
          }
        }
      }
      this._ready = true;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (doInit)`, LogLevel.Error);
    }
  }

  public async IsReady(): Promise<boolean> {
    try {
      let startTime = new Date().getTime();
      while (!this._ready || new Date().getTime() - startTime > 120000) {
        await this.delay(500);
      }
      return this._ready;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (IsReady)`, LogLevel.Error);
      return false;
    }
  }
}