import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { ActionButton, CommandBarButton } from "office-ui-fabric-react/lib/Button";
import { IPlaylist, Playlist, IAsset, ICategory, ITechnology, IAudience, ISearchResult, ISubCategory } from "../../../common/models/Models";
import ImageSelector from "../Atoms/ImageSelector";
import DetailEdit from "../Atoms/DetailEdit";
import PlaylistAssetEdit from "../Atoms/PlaylistAssetEdit";
import AssetEdit from "../Molecules/AssetEdit";
import { MessageBar, MessageBarType } from "office-ui-fabric-react/lib/MessageBar";
import { CustomWebpartSource, Templates, SearchFields } from "../../../common/models/Enums";
import AssetSearchPanel from "./AssetSearchPanel";
import * as strings from "CustomLearningAdminWebPartStrings";
import { SearchBox } from "office-ui-fabric-react/lib/SearchBox";

export interface IPlaylistEditProps {
  placeholderUrl: string;
  playlists: IPlaylist[];
  assets: IAsset[];
  categories: ICategory[];
  technologies: ITechnology[];
  allTechnologies: ITechnology[];
  levels: string[];
  audiences: IAudience[];
  playlistId: string;
  selectedCategory: ICategory;
  selectedSubCategory: ISubCategory;

  editDisabled: boolean;
  close: () => void;
  upsertAsset: (asset: IAsset) => Promise<string>;
  savePlaylist: (playlist: IPlaylist) => Promise<boolean>;
  setEditPlaylistDirty: (dirty: boolean) => void;
}

export interface IPlaylistEditState {
  edit: boolean;
  playlist: IPlaylist;
  editAssetId: string;
  message: string;
  success: boolean;
  playlistChanged: boolean;
  searchAsset: boolean;
  searchValue: string;
  searchResults: ISearchResult[];
  editAsset: boolean;
}

export class PlaylistEditState implements IPlaylistEditState {
  constructor(
    public playlist: IPlaylist = null,
    public edit: boolean = false,
    public playlistChanged: boolean = false,
    public editAssetId: string = "",
    public message: string = "",
    public success: boolean = true,
    public searchAsset: boolean = false,
    public searchValue: string = "",
    public searchResults: ISearchResult[] = null,
    public editAsset: boolean = false
  ) { }
}

export default class PlaylistEdit extends React.Component<IPlaylistEditProps, IPlaylistEditState> {
  private LOG_SOURCE: string = "PlaylistEdit";
  private _reInit: boolean = false;

  constructor(props) {
    super(props);
    try {
      let playlist: IPlaylist;
      if (this.props.playlistId === "0") {
        playlist = new Playlist();
        playlist.Image = props.placeholderUrl;
      } else {
        playlist = lodash.cloneDeep(lodash.find(this.props.playlists, { Id: this.props.playlistId }));
        if (!playlist) {
          playlist = new Playlist();
          playlist.Image = props.placeholderUrl;
        }
      }
      this.state = new PlaylistEditState(
        playlist,
        (this.props.playlistId === "0") ? true : false,
        (this.props.playlistId === "0") ? true : false
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (constructor)`, LogLevel.Error);
    }
  }

  public init = (): void => {
    try {
      let playlist: IPlaylist;
      if (this.props.playlistId === "0") {
        playlist = new Playlist();
      } else {
        playlist = lodash.cloneDeep(lodash.find(this.props.playlists, { Id: this.props.playlistId }));
        if (!playlist)
          playlist = new Playlist();
      }
      this.setState({
        playlist: playlist,
        edit: (this.props.playlistId === "0") ? true : false,
        playlistChanged: (this.props.playlistId === "0") ? true : false
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (init)`, LogLevel.Error);
    }
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistEditProps>, nextState: Readonly<IPlaylistEditState>) {
    try {
      if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
        return false;
      if (nextProps.playlistId !== this.props.playlistId)
        this._reInit = true;
      if (nextState.playlistChanged !== this.state.playlistChanged)
        this.props.setEditPlaylistDirty(nextState.playlistChanged);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (shouldComponentUpdate)`, LogLevel.Error);
    }
    return true;
  }

  public componentDidUpdate() {
    if (this._reInit) {
      this._reInit = false;
      this.init();
    }
  }

  private setImageSource = (imageSrc: string) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      playlist.Image = imageSrc;
      this.setState({
        playlist: playlist,
        playlistChanged: true
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (setImageSource)`, LogLevel.Error);
    }
  }

  private updatePlaylist = (newPlaylist: IPlaylist) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      playlist.Title = newPlaylist.Title;
      playlist.Description = newPlaylist.Description;
      playlist.Category = newPlaylist.Category;
      playlist.SubCategory = newPlaylist.SubCategory;
      playlist.CatId = newPlaylist.CatId;
      playlist.Technology = newPlaylist.Technology;
      playlist.Level = newPlaylist.Level;
      playlist.Audience = newPlaylist.Audience;
      this.setState({
        playlist: playlist,
        playlistChanged: true
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (updatePlaylist)`, LogLevel.Error);
    }
  }

  private moveAsset(array: string[], oldIndex, newIndex) {
    try {
      if (newIndex >= array.length) {
        var k = newIndex - array.length + 1;
        while (k--) {
          array.push(undefined);
        }
      }
      array.splice(newIndex, 0, array.splice(oldIndex, 1)[0]);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (moveAsset)`, LogLevel.Error);
    }
  }

  private moveAssetUp = (index: number) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      this.moveAsset(playlist.Assets, index, index - 1);
      this.setState({
        playlist: playlist,
        playlistChanged: true
      }, () => { this.savePlaylist(); });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (moveAssetUp)`, LogLevel.Error);
    }
  }

  private moveAssetDown = (index: number) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      this.moveAsset(playlist.Assets, index, index + 1);
      this.setState({
        playlist: playlist,
        playlistChanged: true
      }, () => { this.savePlaylist(); });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (moveAssetDown)`, LogLevel.Error);
    }
  }

  private removeAsset = (index: number) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      playlist.Assets.splice(index, 1);
      this.setState({
        playlist: playlist,
        playlistChanged: true
      }, () => { this.savePlaylist(); });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (removeAsset)`, LogLevel.Error);
    }
  }

  private insertAsset = (assetId: string) => {
    try {
      let playlist = lodash.cloneDeep(this.state.playlist);
      playlist.Assets.push(assetId);
      this.setState({
        playlist: playlist,
        playlistChanged: true
      }, () => { this.savePlaylist(); });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (insertAsset)`, LogLevel.Error);
    }
  }

  private doSearch = (searchValue: string): void => {
    let searchResults: ISearchResult[] = [];
    try {
      if (searchValue.length > 0) {
        //Search Assets
        for (let i = 0; i < SearchFields.length; i++) {
          let sp = lodash.filter(this.props.assets, o => (o[SearchFields[i]].toLowerCase().indexOf(searchValue.toLowerCase()) > -1));
          let spResults: ISearchResult[] = [];
          sp.forEach((a) => {
            let result: ISearchResult = { Result: a, Parent: null, Type: Templates.Asset };
            spResults.push(result);
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

  private upsertAsset = async (asset: IAsset): Promise<boolean> => {
    try {
      let editAssetId = lodash.clone(this.state.editAssetId);
      let assetResult = await this.props.upsertAsset(asset);
      let message: string = "";
      let success: boolean = true;
      if (assetResult !== "0") {
        if (asset.Id === "0") {
          editAssetId = assetResult;
          this.insertAsset(assetResult);
        }
        message = strings.PlaylistEditAssetSavedMessage;
      } else {
        message = strings.PlaylistEditAssetSaveFailedMessage;
        success = false;
      }
      this.setState({
        editAssetId: editAssetId,
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
      Logger.write(`${err} - ${this.LOG_SOURCE} (upsertAsset)`, LogLevel.Error);
      return false;
    }
  }

  private savePlaylist = async () => {
    if (this.props.savePlaylist(this.state.playlist)) {
      this.setState({ playlistChanged: false });
    }
  }

  private selectSearchAsset = (subcategoryId: string, playlistId: string, assetId: string) => {
    if (assetId && assetId.length > 0) {
      this.insertAsset(assetId);
      this.closeSearch();
    }
  }

  private closeSearch = () => {
    this.setState({
      searchValue: "",
      searchResults: [],
      searchAsset: false
    });
  }

  private playlistValid(): boolean {
    let valid = true;
    if ((this.state.playlist.Title.length < 1) ||
      (this.state.playlist.Description.length < 1) ||
      (this.state.playlist.Image.length < 1) ||
      (this.state.playlist.Category.length < 1) ||
      (this.state.playlist.SubCategory.length < 1) ||
      (this.state.playlist.Level.length < 1) ||
      (this.state.playlist.Audience.length < 1)
    )
      valid = false;

    return valid;
  }

  private getPlaylistCommandItems = (): JSX.Element[] => {
    let retVal: Array<JSX.Element> = [];
    try {
      retVal.push(<CommandBarButton text={strings.PlaylistEditAssetNewLabel} iconProps={{ iconName: "Add" }} disabled={(this.props.playlistId === "0")} onClick={() => this.setState({ editAssetId: "0" })} />);
      retVal.push(<SearchBox placeholder={strings.AssetSearchPlaceHolderLabel} onSearch={this.doSearch} onClear={() => { this.doSearch(""); }} />);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getPlaylistCommandItems)`, LogLevel.Error);
    }
    return retVal;
  }

  private renderPlaylistButtons = () => {
    let retVal = [];
    try {
      let edit = <ActionButton iconProps={{ iconName: 'Edit' }} text={strings.PlaylistEditEditLabel} onClick={() => { this.setState({ edit: true }); }} />;
      let save = <ActionButton iconProps={{ iconName: 'Save' }} text={strings.PlaylistEditSaveLabel} disabled={!this.state.playlistChanged || !this.playlistValid()} onClick={this.savePlaylist} />;
      let cancel = <ActionButton iconProps={{ iconName: 'ChromeClose' }} text={strings.PlaylistEditCancelLabel} onClick={this.init} />;
      let close = <ActionButton iconProps={{ iconName: 'ChromeClose' }} text={strings.PlaylistEditCloseLabel} onClick={this.props.close} />;
      if (!this.props.editDisabled) {
        if (!this.state.edit) {
          retVal.push(edit);
          retVal.push(close);
        } else {
          retVal.push(save);
          if (this.props.playlistId === "0") {
            retVal.push(close);
          } else {
            retVal.push(cancel);
          }
        }
      } else {
        retVal.push(close);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (renderPlaylistButtons)`, LogLevel.Error);
    }
    return retVal;
  }

  private playlistHeader = () => {
    let header: string = "";
    try {
      if (this.state.playlist.Id === "0") {
        header = strings.PlaylistEditCreatePlaylistHeader;
      } else {
        header = `${strings.PlaylistEditPlaylistDetailsHeader} ${this.state.playlist.Title}`;
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (playlistHeader)`, LogLevel.Error);
    }
    return header;
  }

  public render(): React.ReactElement<IPlaylistEditProps> {
    try {
      return (
        <div>
          <div className="adm-content-section">
            <h2>{this.playlistHeader()}</h2>
            <div className="adm-itemaction">
              {this.renderPlaylistButtons()}
            </div>
            <div className="adm-itemedit">
              <div className="adm-itemleft">
                <ImageSelector
                  placeholderUrl={this.props.placeholderUrl}
                  imageSource={this.state.playlist.Image}
                  disabled={!this.state.edit}
                  setImageSource={this.setImageSource}
                />
              </div>
              <div className="adm-itemright">
                <DetailEdit
                  template={Templates.Playlist}
                  selectedCategory={this.props.selectedCategory}
                  selectedSubCategory={this.props.selectedSubCategory}
                  categories={this.props.categories}
                  technologies={this.props.allTechnologies}
                  levels={this.props.levels}
                  audiences={this.props.audiences}
                  detail={this.state.playlist}
                  updateDetail={this.updatePlaylist}
                  edit={this.state.edit}
                />
              </div>
            </div>
          </div>
          <div className="adm-content-section">
            <h2>{strings.PlaylistEditPlaylistAssetsHeader}</h2>
            {(this.state.message !== "") &&
              <MessageBar
                messageBarType={(this.state.success) ? MessageBarType.success : MessageBarType.error}
                isMultiline={false}
                onDismiss={() => { this.setState({ message: "", success: true }); }}
                dismissButtonAriaLabel={strings.CloseButton}>
                {this.state.message}
              </MessageBar>
            }
            {!this.props.editDisabled &&
              <div className="cmdbar-beta">
                {this.getPlaylistCommandItems()}
              </div>
            }
            {(this.state.editAssetId === "0") &&
              <AssetEdit
                assets={this.props.assets}
                categories={this.props.categories}
                technologies={this.props.allTechnologies}
                levels={this.props.levels}
                audiences={this.props.audiences}
                assetId="0"
                selectedCategory={this.props.selectedCategory}
                selectedSubCategory={this.props.selectedSubCategory}
                detail={this.state.playlist}
                cancel={() => { this.setState({ editAssetId: "" }); }}
                save={this.upsertAsset}
                edit={true}
              />
            }
            {this.state.searchResults && (this.state.searchResults.length > 0) && (this.state.searchResults[0].Result.Id !== "0") &&
              <AssetSearchPanel
                searchResults={this.state.searchResults}
                loadSearchResult={this.selectSearchAsset}
              />
            }
            {this.state.searchResults && (this.state.searchResults.length > 0) && (this.state.searchResults[0].Result.Id === "0") &&
              <p>{this.state.searchResults[0].Result.Title}</p>
            }
            {this.state.playlist.Assets && this.state.playlist.Assets.length > 0 && this.state.playlist.Assets.map((a, index) => {
              let asset = lodash.find(this.props.assets, { Id: a });
              return (
                <div className="learningwrapper">
                  <PlaylistAssetEdit
                    assetIndex={index}
                    assetTotal={this.state.playlist.Assets.length - 1}
                    asset={asset}
                    editDisabled={this.props.editDisabled || (asset && asset.Source !== CustomWebpartSource.Tenant)}
                    allDisabled={this.props.editDisabled}
                    editAssetId={this.state.editAssetId}
                    editAsset={this.state.editAsset}
                    edit={() => { this.setState({ editAssetId: a, editAsset: true }); }}
                    moveUp={() => { this.moveAssetUp(index); }}
                    moveDown={() => { this.moveAssetDown(index); }}
                    remove={() => { this.removeAsset(index); }}
                    select={() => { this.setState({ editAssetId: a, editAsset: false }); }}
                  />
                  {asset && (this.state.editAssetId === asset.Id) &&
                    <AssetEdit
                      assets={this.props.assets}
                      categories={this.props.categories}
                      technologies={this.props.allTechnologies}
                      levels={this.props.levels}
                      audiences={this.props.audiences}
                      assetId={asset.Id}
                      selectedCategory={this.props.selectedCategory}
                      selectedSubCategory={this.props.selectedSubCategory}

                      cancel={() => { this.setState({ editAssetId: "" }); }}
                      save={this.upsertAsset}
                      edit={this.state.editAsset}
                    />
                  }
                </div>
              );
            })}
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
