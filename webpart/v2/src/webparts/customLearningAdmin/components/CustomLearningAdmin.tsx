import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { ICustomConfig, IPlaylist, SubCat, IAsset, ITechnology, ICategory, ICustomSubCategories } from "../../common/models/Models";
import { AdminNavigationType } from "../../common/models/Enums";
import Category from "./Templates/Category";
import Technology from "./Templates/Technology";
import AdminMenu from "./Atoms/AdminMenu";
import styles from "../../common/CustomLearningCommon.module.scss";
import { PivotItem } from "office-ui-fabric-react/lib/Pivot";

export interface ICustomLearningAdminProps {
  config: ICustomConfig;
  customSubCategories: ICustomSubCategories;
  cdnBase: string;
  categories: ICategory[];
  technologies: ITechnology[];
  allTechnologies: ITechnology[];
  playlistsAll: IPlaylist[];
  assetsAll: IAsset[];
  saveConfig: (newConfig: ICustomConfig) => Promise<void>;
  upsertSubCategories: (newSubCategories: ICustomSubCategories) => Promise<string>;
  upsertPlaylist: (playlist: IPlaylist) => Promise<string>;
  deletePlaylist: (playlistId: string) => Promise<void>;
  upsertAsset: (asset: IAsset) => Promise<string>;
  appPartPage: boolean;
}

export interface ICustomLearningAdminState {
  tabSelected: string;
}

export class CustomLearningAdminState implements ICustomLearningAdminState {
  constructor(
    public tabSelected: string = AdminNavigationType.Category
  ) { }
}

export default class CustomLearningAdmin extends React.Component<ICustomLearningAdminProps, ICustomLearningAdminState> {
  private LOG_SOURCE: string = "CustomLearningAdmin";

  constructor(props) {
    super(props);
    this.state = new CustomLearningAdminState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ICustomLearningAdminProps>, nextState: Readonly<ICustomLearningAdminState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private selectTab = (tab: PivotItem): void => {
    if (tab.props.linkText != this.state.tabSelected) {
      this.setState({
        tabSelected: tab.props.linkText
      });
    }
  }

  private updateTechnologyVisibility = (techName: string, subTech: string, exists: boolean): void => {
    try {
      let hiddenTechnology: string[] = lodash.cloneDeep(this.props.config.HiddenTechnology);
      let hiddenSubject: string[] = lodash.cloneDeep(this.props.config.HiddenSubject);
      if (exists) {
        //Add to hidden list
        if (!subTech || subTech.length < 1) {
          hiddenTechnology.push(techName);
        } else {
          hiddenSubject.push(subTech);
        }
      } else {
        //Remove from hidden list
        if (!subTech || subTech.length < 1) {
          lodash.pull(hiddenTechnology, techName);
        } else {
          lodash.pull(hiddenSubject, subTech);
        }
      }
      //Save Config Changes
      let newConfig = lodash.cloneDeep(this.props.config);
      newConfig.HiddenTechnology = hiddenTechnology;
      newConfig.HiddenSubject = hiddenSubject;
      this.props.saveConfig(newConfig);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (updateTechnologyVisibility)`, LogLevel.Error);
    }
  }

  private updateSubCategoryVisibility = (subCategory: string, exists: boolean) => {
    try {
      let hiddenSubCategory: string[] = lodash.cloneDeep(this.props.config.HiddenSubCategories);
      if (exists) {
        //Add to hidden list
        hiddenSubCategory.push(subCategory);
      } else {
        //Remove from hidden list
        lodash.pull(hiddenSubCategory, subCategory);
      }
      //Save Config Changes
      let newConfig = lodash.cloneDeep(this.props.config);
      newConfig.HiddenSubCategories = hiddenSubCategory;
      this.props.saveConfig(newConfig);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (updateSubCategoryVisibility)`, LogLevel.Error);
    }
  }

  private updatePlaylistVisibility = (playlistId: string, exists: boolean): void => {
    try {
      let hiddenPlaylistsIds: string[] = lodash.cloneDeep(this.props.config.HiddenPlaylistsIds);
      if (exists) {
        //Add to hidden list
        hiddenPlaylistsIds.push(playlistId);
      } else {
        //Remove from hidden list
        lodash.pull(hiddenPlaylistsIds, playlistId);
      }
      //Save Config Changes
      let newConfig = lodash.cloneDeep(this.props.config);
      newConfig.HiddenPlaylistsIds = hiddenPlaylistsIds;
      this.props.saveConfig(newConfig);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (updatePlaylistVisibility)`, LogLevel.Error);
    }
  }

  private upsertSubCategory = (edit: boolean, categoryId: string, subCategoryId: string, subCategoryName: string) => {
    try {
      let customSubCategories = lodash.cloneDeep(this.props.customSubCategories);
      if (!customSubCategories.CustomSubcategories || customSubCategories.CustomSubcategories.length < 1) {
        customSubCategories.CustomSubcategories = [];
        for (let i = 0; i < this.props.config.CachedMetadata.Categories.length; i++) {
          let category = lodash.clone(this.props.config.CachedMetadata.Categories[i]);
          category.SubCategories = [];
          customSubCategories.CustomSubcategories.push(category);
        }
      }
      if (edit) {
        let foundCategory = lodash.find(customSubCategories.CustomSubcategories, { Id: categoryId });
        let foundSubCategory = lodash.find(foundCategory.SubCategories, { Id: subCategoryId });
        foundSubCategory.Name = subCategoryName;
      } else {
        let foundCategory = lodash.find(customSubCategories.CustomSubcategories, { Id: categoryId });
        let newSubCategory = new SubCat();
        newSubCategory.Image = `${this.props.cdnBase}images/categories/customfeatured.png`;
        newSubCategory.Name = subCategoryName;
        foundCategory.SubCategories.push(newSubCategory);
      }
      this.props.upsertSubCategories(customSubCategories);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertSubCategory)`, LogLevel.Error);
    }
  }

  private deleteSubCategory = (categoryId: string, subCategoryId: string) => {
    try {
      let customSubCategories = lodash.cloneDeep(this.props.customSubCategories);
      let foundCategory = lodash.find(customSubCategories.CustomSubcategories, { Id: categoryId });
      lodash.remove(foundCategory.SubCategories, { Id: subCategoryId });
      this.props.upsertSubCategories(customSubCategories);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (deleteSubCategory)`, LogLevel.Error);
    }
  }

  private getContainer(): any {
    let element: any;
    try {
      switch (this.state.tabSelected) {
        case AdminNavigationType.Category:
          element = <Category
            placeholderUrl={`${this.props.cdnBase}images/playlists/playlist_bw.png`}
            config={this.props.config}
            technologies={this.props.technologies}
            allTechnologies={this.props.allTechnologies}
            categories={this.props.categories}
            playlists={this.props.playlistsAll}
            assets={this.props.assetsAll}
            updatePlaylistVisibility={this.updatePlaylistVisibility}
            upsertSubCategory={this.upsertSubCategory}
            upsertPlaylist={this.props.upsertPlaylist}
            upsertAsset={this.props.upsertAsset}
            deletePlaylist={this.props.deletePlaylist}
            updateSubcategory={this.updateSubCategoryVisibility}
            deleteSubcategory={this.deleteSubCategory}
          />;
          break;
        default:
          element = <Technology
            technologies={this.props.technologies}
            hiddenTech={this.props.config.HiddenTechnology}
            hiddenSub={this.props.config.HiddenSubject}
            updateTechnology={this.updateTechnologyVisibility}
          />;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getContainer)`, LogLevel.Error);
    }
    return element;
  }

  public render(): React.ReactElement<ICustomLearningAdminProps> {
    try {
      return (
        <div className={`${styles.customLearning} ${(this.props.appPartPage) ? styles.appPartPage : ""}`}>
          <AdminMenu
            selectTab={this.selectTab} />
          {this.getContainer()}
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
