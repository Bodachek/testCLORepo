import { IWebPartContext } from '@microsoft/sp-webpart-base';
import { CustomWebpartSource, Roles } from './Enums';
import * as uuidv4 from "uuid/v4";

export interface IChoice {
  key: string;
  text: string;
}

export interface ITechnology {
  Name: string;
  Image: string;
  Subjects: string[];
}

export interface IMetadataEntry {
  Name: string;
  Image: string;
}

//Additional Count parameter is used to manage category/subcategory visbility
export interface ICategory {
  Id: string;
  Name: string;
  Security: string;
  Image: string;
  Type: string;
  Technology: string;
  SubCategories: ISubCategory[];
  Source: string;
  Count?: number;
}

//Additional Count parameter is used to manage category/subcategory visbility
export interface ISubCategory extends ICategory{}

export class SubCat implements ISubCategory{
  constructor(
    public Id: string = uuidv4(),
    public Name: string = "",
    public Security: string = Roles.Visitors,
    public Image: string = "",
    public Type: string = "SubCategory",
    public Technology: string = "",
    public SubCategories: ISubCategory[] = [],
    public Source: string = CustomWebpartSource.Tenant,
    public Count: number = 0
  ){}
}

export interface IAudience extends IMetadataEntry{}
export interface IPath extends IMetadataEntry{}

export interface IMetadata {
  Technologies: ITechnology[];
  Categories: ICategory[];
  Audiences: IAudience[];
  Sources: string[];
  Levels: string[];
  StatusTags: string[];
  Telemetry: { AppInsightKey: string;};
}

export interface ICustomSubCategories {
  Id: string;
  eTag: string;
  CustomSubcategories: ICategory[];
}

export class CustomSubCategories implements ICustomSubCategories {
  constructor(
    public Id: string = "0",
    public eTag: string = "",
    public CustomSubcategories: ICategory[] = []
  ) {}
}

export interface ICustomConfig {
  Id: number;
  eTag: string;
  HiddenTechnology: string[];
  HiddenSubject: string[];
  HiddenPlaylistsIds: string[];
  HiddenSubCategories: string[];
  CachedMetadata: IMetadata;
  CachedPlaylists: IPlaylist[];
  CachedAssets: IAsset[];
  LastUpdated: Date;
}

export class CustomConfig implements ICustomConfig{
  constructor(
    public Id: number = 0,
    public eTag: string = "",
    public HiddenTechnology: string[] = [],
    public HiddenSubject: string[] = [],
    public HiddenPlaylistsIds: string[] = [],
    public HiddenSubCategories: string[] = [],
    public CachedMetadata: IMetadata = null,
    public CachedPlaylists: IPlaylist[] = [],
    public CachedAssets: IAsset[] = [],
    public LastUpdated: Date = null
  ){}
}

export interface IPlaylist {
  ['@odata.etag']?: string;
  Id: string;
  Title: string;
  Image: string;
  Level: string;
  Audience: string;
  Technology: string;
  Subject: string;
  Category: string;
  SubCategory: string;
  Source: string;
  Assets: string[];
  CatId: string;
  Description: string;
  StatusTag: string;
}

export class Playlist implements IPlaylist {
  constructor(
    public Id: string = "0",
    public Title: string = "",
    public Image: string = "",
    public Level: string = "",
    public Audience: string = "",
    public Technology: string = "",
    public Subject: string = "",
    public Category: string = "",
    public SubCategory: string = "",
    public Source: string = CustomWebpartSource.Tenant,
    public Assets: string[] = [],
    public CatId: string = "",
    public Description: string = "",
    public StatusTag: string = ""
  ){}
}

export interface IAsset {
  ['@odata.etag']?: string;
  Id: string;
  Title: string;
  Description: string;
  Url: string;
  Level: string;
  Audience: string;
  Technology: string;
  Subject: string;
  Category: string;
  SubCategory: string;
  Source: string;
  CatId: string;
  MediaId: string;
  StatusTag: string;
}

export class Asset implements IAsset {
  constructor(
    public Id: string = "0",
    public Title: string = "",
    public Description: string = "",
    public Url: string = "",
    public Level: string = "",
    public Audience: string = "",
    public Technology: string = "",
    public Subject: string = "",
    public Category: string = "",
    public SubCategory: string = "",
    public Source: string = CustomWebpartSource.Tenant,
    public CatId: string = "",
    public MediaId: string = "",
    public StatusTag: string = ""
  ) {}
}

export interface IFilter {
  Audience: string[];
  Level: string[];
}

export class Filter implements IFilter{
  constructor(
    public Audience: string[] = [],
    public Level: string[] = []
  ){}
}

export interface IFilterValue {
  Key: string;
  Value: string;
}

export class FilterValue implements IFilterValue{
  constructor(
    public Key: string = "",
    public Value: string = ""
  ){}
}

export interface IHistoryItem{
  Id: string;
  Name: string;
  Template: string;
}

export class HistoryItem implements IHistoryItem {
  constructor(
    public Id: string = null,
    public Name: string = null,
    public Template: string = null
  ){}
}

export interface IButtonType {
  Class: string;
  SVG: string;
}

export interface ISearchResult {
  Type: string;
  Parent: IPlaylist | ISubCategory;
  Result: IPlaylist | IAsset;
}


