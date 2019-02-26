import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IAsset, ISearchResult } from "../../../common/models/Models";
import HeaderPlaylistPanel from "../Molecules/HeaderPlaylistPanel";
import LinkPanel from "../Atoms/LinkPanel";
import SearchPanel from "../Atoms/SearchPanel";

export interface IHeaderPanelProps {
  userSecurity: string;
  panelOpen: string;
  playlistTitle: string;
  assets: IAsset[];
  assetClick: (assetId: string) => void;
  currentAssetId: string;
  adminPlaylistClick: () => void;
  linkUrl: string;
  doSearch: (searchValue: string) => void;
  searchResults: ISearchResult[];
  loadSearchResult: (subcategoryId: string, playlistId: string, assetId: string) => void;
}

export interface IHeaderPanelState { }

export class HeaderPanelState implements IHeaderPanelState {
  constructor() { }
}

export default class HeaderPanel extends React.Component<IHeaderPanelProps, IHeaderPanelState> {
  private LOG_SOURCE: string = "HeaderPanel";

  constructor(props) {
    super(props);
    this.state = new HeaderPanelState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IHeaderPanelProps>, nextState: Readonly<IHeaderPanelState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public componentDidUpdate() {
    try {
      if (this.props.panelOpen !== "Search" && (this.props.searchResults.length > 0)) {
        this.props.doSearch("");
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (componentDidUpdate)`, LogLevel.Error);
    }
  }

  public render(): React.ReactElement<IHeaderPanelProps> {
    try {
      let element: any;
      switch (this.props.panelOpen) {
        case "Burger":
          element = <HeaderPlaylistPanel
            userSecurity={this.props.userSecurity}
            panelOpen={this.props.panelOpen}
            playlistTitle={this.props.playlistTitle}
            assets={this.props.assets}
            assetClick={this.props.assetClick}
            currentAssetId={this.props.currentAssetId}
            adminPlaylistClick={this.props.adminPlaylistClick} />;
          break;
        case "Link":
          element = <LinkPanel
            panelOpen={this.props.panelOpen}
            linkUrl={this.props.linkUrl} />;
          break;
        case "Search":
          element = <SearchPanel
            panelOpen={this.props.panelOpen}
            doSearch={this.props.doSearch}
            searchResults={this.props.searchResults}
            loadSearchResult={this.props.loadSearchResult} />;
          break;
        default:
          element = null;
      }
      return (
        element
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
