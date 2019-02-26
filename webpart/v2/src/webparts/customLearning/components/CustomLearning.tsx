import * as React from 'react';
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from 'lodash';
import { ICustomConfig, ISubCategory, IPlaylist, ICategory, IHistoryItem, HistoryItem, IAsset, IFilterValue, IFilter, FilterValue, Filter, ISearchResult, Playlist } from '../../common/models/Models';
import { Templates, FilterTypes, WebpartMode, CustomWebpartSource, SearchFields } from '../../common/models/Enums';
import Categories from './Molecules/Categories';
import SubCategories from './Templates/SubCategories';
import LearningHeader from './Templates/LearningHeader';
import AssetView from './Atoms/AssetView';
import PlaylistControl from "./Molecules/PlaylistControl";
import styles from "../../common/CustomLearningCommon.module.scss";
import * as strings from "CustomLearningWebPartStrings";

export interface ICustomLearningProps {
  userSecurity: string;
  config: ICustomConfig;
  webpartMode: string;
  startType: string;
  startLocation: string;
  startAsset: string;
  baseAdminUrl: string;
  baseViewerUrl: string;
  appPartPage: boolean;
  webpartTitle: string;
}

export interface ICustomLearningState {
  template: string;
  templateId: string;
  parent: ICategory | ISubCategory;
  detail: ICategory[] | ISubCategory[] | IPlaylist[] | IPlaylist;
  assets: IAsset[];
  currentAsset: IAsset;
  history: IHistoryItem[];
  filter: IFilter;
  filterValues: IFilterValue[];
  url: string;
  searchValue: string;
  searchResults: ISearchResult[];
}

export class CustomLearningState implements ICustomLearningState {
  constructor(
    public template: string = "",
    public templateId: string = "",
    public parent: ICategory | ISubCategory = null,
    public detail: ICategory[] | ISubCategory[] | IPlaylist[] | IPlaylist = null,
    public assets: IAsset[] = null,
    public currentAsset: IAsset = null,
    public history: IHistoryItem[] = [],
    public filter: IFilter = new Filter(),
    public filterValues: IFilterValue[] = [],
    public url: string = "",
    public searchValue: string = "",
    public searchResults: ISearchResult[] = []
  ) { }
}

export default class CustomLearning extends React.Component<ICustomLearningProps, ICustomLearningState> {
  private LOG_SOURCE = "CustomLearning";
  private _reInit: boolean = false;

  constructor(props) {
    super(props);
    this.state = new CustomLearningState();
    this.init();
  }

  private findParentCategory(id: string, categories: ICategory[], lastParent: Array<ICategory | ISubCategory>): Array<ICategory | ISubCategory> {
    let parent: Array<ICategory | ISubCategory> = lastParent;
    try {
      for (let i = 0; i < categories.length; i++) {
        if (categories[i].SubCategories.length > 0) {
          let found: boolean = false;
          for (let j = 0; j < categories[i].SubCategories.length; j++) {
            if (categories[i].SubCategories[j].Id == id) {
              found = true;
              parent.push(categories[i].SubCategories[j]);
              break;
            }
          }
          if (found) {
            parent.push(categories[i]);
            break;
          }
        }
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (findParentCategory)`, LogLevel.Error);
    }
    return parent;
  }

  private init() {
    if (this.props.webpartMode === WebpartMode.contentonly) { return; }
    try {
      //During constructor, update state directly.
      this.state.history.push(new HistoryItem("", strings.NavigationHome, ""));
      switch (this.props.startType) {
        case Templates.SubCategory:
          let c = this.findParentCategory(this.props.startLocation, this.props.config.CachedMetadata.Categories, []);
          if (c.length > 0) {
            //For subcategories, don't include displayed subcategory.
            for (let i = c.length - 1; i > 0; i--) {
              this.state.history.push(new HistoryItem(c[i].Id, c[i].Name, c[i].Type));
            }
          }
          break;
        case Templates.Playlist:
          let p = lodash.find(this.props.config.CachedPlaylists, { Id: this.props.startLocation });
          let pc = this.findParentCategory(p.CatId, this.props.config.CachedMetadata.Categories, []);
          if (pc.length > 0) {
            for (let i = pc.length - 1; i >= 0; i--) {
              this.state.history.push(new HistoryItem(pc[i].Id, pc[i].Name, pc[i].Type));
            }
          }
          break;
        default:
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (init)`, LogLevel.Error);
    }
  }

  public componentDidUpdate() {
    if (this._reInit) {
      this._reInit = false;
      this.loadDetail(this.props.startType, this.props.startLocation, []);
    }
  }

  public shouldComponentUpdate(nextProps: Readonly<ICustomLearningProps>, nextState: Readonly<ICustomLearningState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    if (this.props.startType != nextProps.startType || this.props.startLocation != nextProps.startLocation)
      this._reInit = true;
    return true;
  }

  private handleBeforeUnload = (event: BeforeUnloadEvent) => {
    event.returnValue = "Are you sure you want to leave this page? ";
    return "Are you sure you want to leave this page? ";
  }

  public componentDidMount() {
    this.loadDetail(this.props.startType, this.props.startLocation, this.state.history);

    //Track back button
    window.addEventListener("beforeunload", this.handleBeforeUnload, true);
  }

  public componentWillUnmount() {
    //Release windows events
    window.removeEventListener("beforeunload", this.handleBeforeUnload);
  }

  private getFilterValues(subcategory: ISubCategory): IFilterValue[] {
    let filterValues: IFilterValue[] = [];
    try {
      let checkPlaylists = (playlists: IPlaylist[]): void => {
        for (let i = 0; i < playlists.length; i++) {
          let foundAudience = lodash.findIndex(filterValues, { Key: FilterTypes.Audience, Value: playlists[i].Audience });
          if (foundAudience < 0)
            filterValues.push(new FilterValue(FilterTypes.Audience, playlists[i].Audience));
          let foundLevel = lodash.findIndex(filterValues, { Key: FilterTypes.Level, Value: playlists[i].Level });
          if (foundLevel < 0)
            filterValues.push(new FilterValue(FilterTypes.Level, playlists[i].Level));
        }
      };

      let subs: ISubCategory[] = (subcategory.SubCategories.length == 0) ? [subcategory] : subcategory.SubCategories;
      for (let i = 0; i < subs.length; i++) {
        let pl = lodash.filter(this.props.config.CachedPlaylists, { CatId: subs[i].Id });
        if (pl.length > 0)
          checkPlaylists(pl);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getFilterValues)`, LogLevel.Error);
    }

    return filterValues;
  }

  private filterPlaylists = (playlists: IPlaylist[], filter: IFilter): IPlaylist[] => {
    try {
      let filtered: IPlaylist[] = playlists.filter((pl) => {
        let retVal = true;
        if (filter.Level.length > 0)
          retVal = lodash.includes(filter.Level, pl.Level);
        if (filter.Audience.length > 0 && retVal)
          retVal = lodash.includes(filter.Audience, pl.Audience);
        return retVal;
      });
      return filtered;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (filterPlaylists)`, LogLevel.Error);
      return [];
    }
  }

  public loadDetail = (template: string, templateId: string, history?: IHistoryItem[], filter?: IFilter, assetId?: string): void => {
    try {
      if (!history) {
        history = lodash.cloneDeep(this.state.history);
      }
      let updateHistory: boolean = true;
      if (!filter) {
        filter = new Filter();
      }else{
        updateHistory = false;
      }

      //redirect if not full page and clicking on a link
      if (this.props.webpartMode !== WebpartMode.full && (templateId && templateId != "") && (history.length > 0)) {
        document.location.href = `${this.props.baseViewerUrl}?${template}=${templateId}`;
      }
      //Continue loading
      let parent: ICategory | ISubCategory;
      let detail: ICategory[] | ISubCategory[] | IPlaylist[] | IPlaylist;
      let assets: IAsset[] = null;
      let currentAsset: IAsset = null;
      let filterValues: IFilterValue[] = lodash.cloneDeep(this.state.filterValues);
      let url: string = this.props.baseViewerUrl;
      switch (template) {
        case Templates.Category:
          detail = lodash.filter(this.props.config.CachedMetadata.Categories, { Id: templateId });
          history.push(new HistoryItem(detail[0].Id, detail[0].Name, detail[0].Type));
          if (detail.length === 1) {
            url = `${url}?category=${detail[0].Id}`;
          }
          break;
        case Templates.SubCategory:
        case Templates.Playlists:
          let subCategory = this.findParentCategory(templateId, this.props.config.CachedMetadata.Categories, []);
          parent = subCategory[0];
          filterValues = this.getFilterValues(subCategory[0]);
          if (subCategory[0].SubCategories.length > 0) {
            template = Templates.SubCategory;
            detail = subCategory[0].SubCategories;
          } else {
            template = Templates.Playlists;
            detail = lodash.filter(this.props.config.CachedPlaylists, { CatId: subCategory[0].Id });
            detail = this.filterPlaylists(detail, filter);
          }
          if (updateHistory){
            history.push(new HistoryItem(subCategory[0].Id, subCategory[0].Name, subCategory[0].Type));
          }
          
          url = `${url}?subcategory=${subCategory[0].Id}`;
          break;
        case Templates.Playlist:
          detail = lodash.find(this.props.config.CachedPlaylists, { Id: templateId });
          history.push(new HistoryItem(detail.Id, detail.Title, Templates.Playlist));
          url = `${url}?playlist=${detail.Id}`;
          assets = [];
          for (let i = 0; i < (detail as IPlaylist).Assets.length; i++) {
            let a = lodash.find(this.props.config.CachedAssets, { Id: (detail as IPlaylist).Assets[i] });
            if (a)
              assets.push(a);
          }
          break;
        default:
          detail = this.props.config.CachedMetadata.Categories;
          //this.state.history.push(new HistoryItem("", strings.NavigationHome, ""));
          template = Templates.Category;
      }

      this.setState({
        template: template,
        templateId: templateId,
        parent: parent,
        detail: detail,
        assets: assets,
        currentAsset: currentAsset,
        history: history,
        filterValues: filterValues,
        filter: filter,
        url: url
      }, () => {
        //For playlist, initialize the starting asset.
        if (this.state.template === Templates.Playlist) {
          if (this.state.assets.length > 0) {
            if (!assetId) {
              if (this.props.startLocation === (this.state.detail as IPlaylist).Id && (this.props.startAsset && this.props.startAsset.length > 0)) {
                assetId = this.props.startAsset;
              } else {
                assetId = this.state.assets[0].Id;
              }
            }
            this.selectAsset(assetId);
          }
        }
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (loadDetail)`, LogLevel.Error);
    }
  }

  private getContainer(): any {
    let element: any;
    try {
      switch (this.state.template) {
        case Templates.Category:
          element = <Categories
            detail={this.state.detail as ICategory[]}
            selectItem={this.loadDetail}
          />;
          break;
        case Templates.SubCategory:
        case Templates.Playlists:
          element = <SubCategories
            parent={this.state.parent}
            template={this.state.template}
            detail={this.state.detail as ISubCategory[] | IPlaylist[]}
            filter={this.state.filter}
            filterValues={this.state.filterValues}
            selectItem={this.loadDetail}
            setFilter={this.setFilter}
          />;
          break;
        case Templates.Playlist:
          element = <AssetView
            asset={this.state.currentAsset}
            assets={this.state.assets}
            selectAsset={this.selectAsset}
          />;
          break;
        default:
          element = <Categories
            detail={this.state.detail as ICategory[]}
            selectItem={this.loadDetail}
          />;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getContainer)`, LogLevel.Error);
    }
    return element;
  }

  private historyClick = (template: string, templateId: string, nav?: boolean): void => {
    try {
      let history = lodash.cloneDeep(this.state.history);
      if (nav) {
        //Update history to remove items
        if (templateId === "") {
          history = [new HistoryItem("", strings.NavigationHome, "")];
        } else {
          let idx = lodash.findIndex(history, { Id: templateId });
          history.splice(idx, (history.length - idx));
        }
      }
      this.loadDetail(template, templateId, history);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (historyClick)`, LogLevel.Error);
    }
  }

  private selectAsset = (assetId: string): void => {
    try {
      let currentAsset = lodash.find(this.state.assets, { Id: assetId });
      if (!lodash.isEqual(currentAsset, this.state.currentAsset)) {
        // if (currentAsset.Source === CustomWebpartSource.Tenant) {
        //   if (`${document.location.origin}${document.location.pathname}` !== currentAsset.Url)
        //     document.location.href = `${currentAsset.Url}?playlist=${(this.state.detail as IPlaylist).Id}&asset=${currentAsset.Id}`;
        // } else {
        //   if ((`${document.location.origin}${document.location.pathname}` !== this.props.baseViewerUrl) && (this.props.webpartMode !== WebpartMode.full))
        //     document.location.href = `${this.props.baseViewerUrl}?playlist=${(this.state.detail as IPlaylist).Id}&asset=${currentAsset.Id}`;
        // }
        let url: string = `${this.props.baseViewerUrl}?playlist=${(this.state.detail as IPlaylist).Id}&asset=${currentAsset.Id}`;
        this.setState({
          url: url,
          currentAsset: currentAsset
        }, () => {
          document.body.scrollTop = 0;
          document.documentElement.scrollTop = 0;
        });
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (selectAsset)`, LogLevel.Error);
    }
  }

  private setFilter = (filterValue: IFilterValue): void => {
    try {
      let filter: IFilter = lodash.cloneDeep(this.state.filter);
      switch (filterValue.Key) {
        case "Level":
          let levelIdx = lodash.indexOf(filter.Level, filterValue.Value);
          (levelIdx > -1) ?
            filter.Level.splice(levelIdx, 1) :
            filter.Level.push(filterValue.Value);
          break;
        case "Audience":
          let audIdx = lodash.indexOf(filter.Audience, filterValue.Value);
          (audIdx > -1) ?
            filter.Audience.splice(audIdx, 1) :
            filter.Audience.push(filterValue.Value);
          break;
      }

      this.loadDetail(this.state.template, this.state.templateId, this.state.history, filter);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (setFilter)`, LogLevel.Error);
    }
  }

  private doDisableAdvance = (): boolean => {
    try {
      if (!this.state.currentAsset || this.state.assets.length < 1) return true;
      return (this.state.currentAsset.Id === this.state.assets[this.state.assets.length - 1].Id);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (doDisableAdvance)`, LogLevel.Error);
      return true;
    }
  }

  private doDisableBack = (): boolean => {
    try {
      if (!this.state.currentAsset || this.state.assets.length < 1) return true;
      return (this.state.currentAsset.Id === this.state.assets[0].Id);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (doDisableBack)`, LogLevel.Error);
      return true;
    }
  }

  private playlistAdvance = (): void => {
    try {
      let currentIdx = lodash.indexOf(this.state.assets, this.state.currentAsset);
      this.selectAsset(this.state.assets[(currentIdx + 1)].Id);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (playlistAdvance)`, LogLevel.Error);
    }
  }

  private playlistBack = (): void => {
    try {
      let currentIdx = lodash.indexOf(this.state.assets, this.state.currentAsset);
      this.selectAsset(this.state.assets[(currentIdx - 1)].Id);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (playlistBack)`, LogLevel.Error);
    }
  }

  private onAdminPlaylists = (): void => {
    window.open(this.props.baseAdminUrl, '_blank');
  }

  private flattenCategory(category: ICategory[] | ISubCategory[], array: ICategory[] | ISubCategory[] = []): ICategory[] | ISubCategory[] {
    let retArray: ICategory[] | ISubCategory[] = array;
    try {
      category.forEach((c) => {
        let item = lodash.cloneDeep(c);
        item.SubCategories = [];
        retArray.push(item);
        if (c.SubCategories && c.SubCategories.length > 0) {
          let sub = this.flattenCategory(c.SubCategories, retArray);
          retArray = lodash.concat(retArray, sub);
        }
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (flattenCategory)`, LogLevel.Error);
    }
    return retArray;
  }

  private doSearch = (searchValue: string): void => {
    try {
      let searchResults: ISearchResult[] = [];
      if (searchValue.length > 0) {
        //Search Assets
        for (let i = 0; i < SearchFields.length; i++) {
          let sp = lodash.filter(this.props.config.CachedAssets, o => (o[SearchFields[i]].toLowerCase().indexOf(searchValue.toLowerCase()) > -1));
          let spResults: ISearchResult[] = [];
          sp.forEach((a) => {
            let parent: IPlaylist[] = lodash.filter(this.props.config.CachedPlaylists, o => (o.Assets.indexOf(a.Id) > -1));
            parent.forEach((pl) => {
              let result: ISearchResult = { Result: a, Parent: pl, Type: Templates.Asset };
              spResults.push(result);
            });
          });
          searchResults = lodash.concat(searchResults, spResults);
        }
        //searchResults = lodash.uniqBy(searchResults,'Result.Id');
        //Search Playlists
        let flatSubCategories: ICategory[] | ISubCategory[] = this.flattenCategory(this.props.config.CachedMetadata.Categories);
        for (let i = 0; i < SearchFields.length; i++) {
          let sp = lodash.filter(this.props.config.CachedPlaylists, o => (o[SearchFields[i]].toLowerCase().indexOf(searchValue.toLowerCase()) > -1));
          let spResults: ISearchResult[] = [];
          sp.forEach((pl) => {
            let parent: ISubCategory = lodash.find(flatSubCategories, { Id: pl.CatId });
            if (parent) {
              let result: ISearchResult = { Result: pl, Parent: parent, Type: Templates.Playlist };
              spResults.push(result);
            }
          });

          searchResults = lodash.concat(searchResults, spResults);
        }
        searchResults = lodash.uniqBy(searchResults, "Result.Id");
        searchResults = lodash.sortBy(searchResults, "Result.Title");
        if (searchResults.length === 0)
          searchResults.push({ Result: new Playlist("0", strings.NoSearchResults), Parent: null, Type: null });
      }
      this.setState({
        searchValue: searchValue,
        searchResults: searchResults
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (doSearch)`, LogLevel.Error);
    }
  }

  private loadSeachResult = (subcategoryId: string, playlistId: string, assetId: string): void => {
    try {
      let history = lodash.clone(this.state.history);
      if (history.length > 1)
        history.splice(1);
      if (playlistId) {
        this.loadDetail(Templates.Playlist, playlistId, history, undefined, assetId);
      } else if (subcategoryId) {
        this.loadDetail(Templates.SubCategory, subcategoryId, history);
      }
      this.setState({
        searchValue: "",
        searchResults: []
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (loadSeachResult)`, LogLevel.Error);
    }
  }

  public render(): React.ReactElement<ICustomLearningProps> {
    if (!this.state.template) return null;
    try {
      return (
        <div className={`${styles.customLearning} ${(this.props.appPartPage) ? styles.appPartPage : ""}`}>
          <LearningHeader
            userSecurity={this.props.userSecurity}
            template={this.state.template}
            detail={((this.state.template === Templates.Playlist) ? this.state.detail : null) as IPlaylist}
            history={this.state.history}
            historyClick={this.historyClick}
            selectAsset={this.selectAsset}
            assets={this.state.assets}
            currentAsset={this.state.currentAsset}
            linkUrl={this.state.url}
            onAdminPlaylists={this.onAdminPlaylists}
            doSearch={this.doSearch}
            searchResults={this.state.searchResults}
            loadSearchResult={this.loadSeachResult}
            webpartMode={this.props.webpartMode}
            webpartTitle={this.props.webpartTitle}
          />
          {(this.state.template === Templates.Playlist) &&
            <PlaylistControl
              playlistTitle={(this.state.detail as IPlaylist).Title}
              onAdvance={this.playlistAdvance}
              disableAdvance={this.doDisableAdvance()}
              onBack={this.playlistBack}
              disableBack={this.doDisableBack()} />
          }
          {this.getContainer()}
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}


// {(this.state.template === Templates.Playlist) &&
//   [<h2>{strings.PlaylistOverviewHeading}</h2>,
//   <PlaylistOverview
//     playlistAssets={this.state.assets}
//     currentAssetId={(this.state.currentAsset) ? this.state.currentAsset.Id : null}
//     assetClick={this.selectAsset}
//   />]
// }