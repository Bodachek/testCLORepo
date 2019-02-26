import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { ICustomConfig, ICategory, ISubCategory, IPlaylist, IAsset, ITechnology } from "../../../common/models/Models";
import CategoryHeading from "../Atoms/CategoryHeading";
import PlaylistItemEdit from "../Atoms/PlaylistItemEdit";
import { CustomWebpartSource } from "../../../common/models/Enums";
import PlaylistEdit from "../Organizms/PlaylistEdit";
import { MessageBar, MessageBarType } from 'office-ui-fabric-react/lib/MessageBar';
import CategoryNav from "../Atoms/CategoryNav";
import * as strings from "CustomLearningAdminWebPartStrings";
import { MessageBarButton } from "office-ui-fabric-react/lib/Button";

export interface IListing {
  heading: ISubCategory;
  playlists: IPlaylist[];
}

export interface ICategoryProps {
  placeholderUrl: string;
  config: ICustomConfig;
  categories: ICategory[];
  technologies: ITechnology[];
  allTechnologies: ITechnology[];
  playlists: IPlaylist[];
  assets: IAsset[];
  updatePlaylistVisibility: (playlistId: string, exists: boolean) => void;
  upsertPlaylist: (playlist: IPlaylist) => Promise<string>;
  upsertAsset: (asset: IAsset) => Promise<string>;
  upsertSubCategory: (edit: boolean, categoryId: string, subCategoryId: string, subCategoryName: string) => void;
  deletePlaylist: (playlistId: string) => Promise<void>;
  updateSubcategory: (subCategory: string, exists: boolean) => void;
  deleteSubcategory: (categoryId: string, subCategoryId: string) => void;
}

export interface ICategoryState {
  selectedCategoryId: string;
  selectedCategoryType: string;
  selectedCategory: ICategory;
  selectedSubCategory: ISubCategory;
  listings: IListing[];
  newSubCategory: string;
  editHeadingId: string;
  editHeadingValue: string;
  editDisabled: boolean;
  editPlaylistId: string;
  editPlaylistDirty: boolean;
  editRedirectSelected: ICategory | ISubCategory;
  message: string;
  success: boolean;
}

export class CategoryState implements ICategoryState {
  constructor(
    public selectedCategoryId: string = "",
    public selectedCategoryType: string = "",
    public selectedCategory: ICategory = null,
    public selectedSubCategory: ISubCategory = null,
    public listings: IListing[] = null,
    public newSubCategory: string = "",
    public editHeadingId: string = "",
    public editHeadingValue: string = "",
    public editDisabled: boolean = false,
    public editPlaylistId: string = "",
    public editPlaylistDirty: boolean = false,
    public editRedirectSelected: ICategory | ISubCategory = null,
    public message: string = "",
    public success: boolean = true
  ) { }
}

export default class Category extends React.Component<ICategoryProps, ICategoryState> {
  private LOG_SOURCE: string = "Category";
  private _reInit: boolean = false;

  constructor(props) {
    super(props);
    this.state = new CategoryState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ICategoryProps>, nextState: Readonly<ICategoryState>) {
    try {
      if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
        return false;
      if (!lodash.isEqual(nextProps.config.CachedMetadata.Categories, this.props.config.CachedMetadata.Categories) ||
        !lodash.isEqual(nextProps.playlists, this.props.playlists))
        this._reInit = true;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (shouldComponentUpdate)`, LogLevel.Error);
    }
    return true;
  }

  public componentDidUpdate() {
    try {
      if (this._reInit) {
        this._reInit = false;
        let selectedCategory = lodash.find(this.props.categories, { Id: this.state.selectedCategory.Id });
        if (!selectedCategory) {
          for (let i = 0; i < this.props.categories.length; i++) {
            for (let j = 0; j < this.props.categories[i].SubCategories.length; j++) {
              if (this.props.categories[i].SubCategories[j].Id === this.state.selectedCategory.Id) {
                selectedCategory = this.props.categories[i].SubCategories[j];
                break;
              }
            }
            if (selectedCategory)
              break;
          }
        }
        if (selectedCategory)
          this.selectCategory(selectedCategory);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (componentDidUpdate)`, LogLevel.Error);
    }
  }

  private selectCategory = (selected: ICategory | ISubCategory): void => {
    try {
      let editPlaylistId = lodash.clone(this.state.editPlaylistId);
      let editPlaylistDirty = lodash.clone(this.state.editPlaylistDirty);
      if (this.state.editPlaylistDirty && this.state.editPlaylistId.length > 0) {
        this.setState({
          editRedirectSelected: selected
        });
        return;
      } else {
        editPlaylistId = "";
        editPlaylistDirty = false;
      }

      let selectedCategoryId: string;
      let selectedCategoryType: string = "SubCategory";
      for (let i = 0; i < this.props.categories.length; i++) {
        if (this.props.categories[i].Id === selected.Id) {
          selectedCategoryId = selected.Id;
          selectedCategoryType = "Category";
        }
      }
      if (!selectedCategoryId) {
        //Assume a subcategory and find parent
        for (let i = 0; i < this.props.categories.length; i++) {
          if (this.props.categories[i].SubCategories.length > 0) {
            for (let j = 0; j < this.props.categories[i].SubCategories.length; j++) {
              if (this.props.categories[i].SubCategories[j].Id === selected.Id) {
                selectedCategoryId = this.props.categories[i].Id;
              }
            }
          }
        }
      }
      //Create listing
      let listings: IListing[] = [];

      if (selected && selected.SubCategories.length > 0) {
        selected.SubCategories.forEach((sub) => {
          let l: IListing = { heading: sub, playlists: null };
          if (sub.SubCategories.length < 1) {
            l.playlists = lodash.filter(this.props.playlists, { CatId: sub.Id });
          }
          listings.push(l);
        });
      } else if (selected) {
        let l: IListing = { heading: selected, playlists: null };
        if (selected.SubCategories.length < 1) {
          l.playlists = lodash.filter(this.props.playlists, { CatId: selected.Id });
        }
        listings.push(l);
      }
      this.setState({
        selectedCategoryId: selectedCategoryId,
        selectedCategoryType: selectedCategoryType,
        selectedCategory: selected,
        listings: listings,
        editPlaylistId: editPlaylistId,
        editPlaylistDirty: editPlaylistDirty,
        editRedirectSelected: null
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (selectCategory)`, LogLevel.Error);
    }
  }

  private saveSubCategory = (Id: string, newValue: string): void => {
    this.props.upsertSubCategory((Id !== "0"), this.state.selectedCategoryId, Id, newValue);

    this.setState({
      editHeadingId: "",
      editHeadingValue: "",
      newSubCategory: ""
    });
  }

  private setEditPlaylistDirty = (dirty: boolean) => {
    this.setState({
      editPlaylistDirty: dirty
    });
  }

  private upsertPlaylist = async (playlist: IPlaylist): Promise<boolean> => {
    try {
      let editPlaylistId = lodash.clone(this.state.editPlaylistId);
      let playlistResult = await this.props.upsertPlaylist(playlist);
      let message: string = "";
      let success: boolean = true;
      if (playlistResult !== "0") {
        if (playlist.Id === "0")
          editPlaylistId = playlistResult;
        message = strings.CategoryPlaylistSavedMessage;
      } else {
        message = strings.CategoryPlaylistSaveFailedMessage;
        success = false;
      }
      this.setState({
        editPlaylistId: editPlaylistId,
        editPlaylistDirty: false,
        message: message,
        success: success
      }, () => {
        if (this.state.message.length > 0) {
          //Auto dismiss message
          window.setTimeout(() => {
            this.setState({
              message: "",
              success: true
            });
          }, 5000);
        }
      });
      return success;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertPlaylist)`, LogLevel.Error);
      return false;
    }
  }

  private editPlaylist = (subcategory: ISubCategory, editPlaylistId: string, editDisabled: boolean) => {
    try {
      let category = lodash.find(this.props.categories, (item) => { return lodash.findIndex(item.SubCategories, { Id: subcategory.Id }) > -1; });
      this.setState({
        editPlaylistId: editPlaylistId,
        editDisabled: editDisabled,
        selectedCategory: category,
        selectedSubCategory: subcategory
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (editPlaylist)`, LogLevel.Error);
    }
  }

  private deleteSubcategory = (subcategory: ISubCategory) => {
    if (subcategory.Count < 1) {
      this.props.deleteSubcategory(this.state.selectedCategoryId, subcategory.Id);
    }
  }

  public render(): React.ReactElement<ICategoryProps> {
    try {
      return (
        <div className="adm-content">
          <div className="adm-navsection-subcat">
            <CategoryNav
              categories={this.props.categories}
              selectedId={(this.state.selectedCategory) ? this.state.selectedCategory.Id : ""}
              onClick={this.selectCategory}
            />
          </div>
          <div className="adm-content-main">
            {this.state.editPlaylistId === "" && this.state.listings && this.state.listings.length > 0 && this.state.listings.map((listing: IListing) => {
              return (
                <div>
                  <CategoryHeading
                    heading={listing.heading.Name}
                    id={listing.heading.Id}
                    playlistCount={listing.heading.Count}
                    editable={listing.heading.Source === CustomWebpartSource.Tenant}
                    visible={this.props.config.HiddenSubCategories.indexOf(listing.heading.Id) < 0}
                    saveSubCategory={this.saveSubCategory}
                    addPlaylist={() => this.editPlaylist(listing.heading, "0", false)}
                    onVisibility={this.props.updateSubcategory}
                    onDelete={() => this.deleteSubcategory(listing.heading)}
                  />
                  <ul className="adm-content-playlist">
                    {listing.playlists && listing.playlists.length > 0 && listing.playlists.map((playlist) => {
                      return (
                        <li>
                          <PlaylistItemEdit
                            playlistId={playlist.Id}
                            playlistTitle={playlist.Title}
                            playlistVisible={this.props.config.HiddenPlaylistsIds.indexOf(playlist.Id) < 0}
                            playlistEditable={playlist.Source === CustomWebpartSource.Tenant}
                            onVisible={this.props.updatePlaylistVisibility}
                            onEdit={() => { this.editPlaylist(listing.heading, playlist.Id, (playlist.Source !== CustomWebpartSource.Tenant)); }}
                            onClick={() => { this.editPlaylist(listing.heading, playlist.Id, (playlist.Source !== CustomWebpartSource.Tenant)); }}
                            onMoveDown={() => { console.error("not implemented"); }}
                            onMoveUp={() => { console.error("not implemented"); }}
                            onDelete={() => { this.props.deletePlaylist(playlist.Id); }}
                          />
                        </li>
                      );
                    })}
                  </ul>
                </div>
              );
            })}
            {this.state.editPlaylistId === "" && this.state.selectedCategory && this.state.selectedCategoryType === "Category" &&
              <div>
                <CategoryHeading
                  heading={strings.CategoryHeading}
                  id="0"
                  playlistCount={1}
                  visible={true}
                  editable={true}
                  saveSubCategory={this.saveSubCategory}
                  onVisibility={() => { }}
                  onDelete={() => { }}
                />
              </div>
            }
            {(this.state.message !== "") &&
              <MessageBar
                messageBarType={(this.state.success) ? MessageBarType.success : MessageBarType.error}
                isMultiline={false}
                onDismiss={() => { this.setState({ message: "", success: true }); }}
                dismissButtonAriaLabel={strings.CloseButton}>
                {this.state.message}
              </MessageBar>
            }
            {this.state.editRedirectSelected && this.state.editPlaylistDirty && this.state.editPlaylistId.length > 0 &&
              <MessageBar
                messageBarType={MessageBarType.blocked}
                actions={
                  <div>
                    <MessageBarButton onClick={() => { this.setState({ editPlaylistId: "", editPlaylistDirty: false }, () => { this.selectCategory(this.state.editRedirectSelected); }); }}>Yes</MessageBarButton>
                    <MessageBarButton onClick={() => { this.setState({ editRedirectSelected: null }); }}>No</MessageBarButton>
                  </div>
                }
              >
                <span>{(this.state.editPlaylistId === "0" ? strings.CategoryNewPlayListMessage : strings.CategoryEditedPlayListMessage)}</span>
              </MessageBar>
            }
            {this.state.editPlaylistId !== "" &&
              <PlaylistEdit
                placeholderUrl={this.props.placeholderUrl}
                playlists={this.props.playlists}
                assets={this.props.assets}
                categories={this.props.categories}
                technologies={this.props.technologies}
                allTechnologies={this.props.allTechnologies}
                levels={this.props.config.CachedMetadata.Levels}
                audiences={this.props.config.CachedMetadata.Audiences}
                playlistId={this.state.editPlaylistId}
                selectedCategory={this.state.selectedCategory}
                selectedSubCategory={this.state.selectedSubCategory}
                editDisabled={this.state.editDisabled}
                close={() => { this.setState({ editPlaylistId: "", editPlaylistDirty: false }); }}
                upsertAsset={this.props.upsertAsset}
                savePlaylist={this.upsertPlaylist}
                setEditPlaylistDirty={this.setEditPlaylistDirty}
              />
            }
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
