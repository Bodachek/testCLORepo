import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IAsset } from "../../../common/models/Models";
import AssetItem from "../Atoms/AssetItem";
import * as strings from "CustomLearningWebPartStrings";

export interface IPlaylistOverviewProps {
  playlistAssets: IAsset[];
  currentAssetId: string;
  assetClick: (assetId: string) => void;
}

export interface IPlaylistOverviewState {
}

export class PlaylistOverviewState implements IPlaylistOverviewState {
  constructor() { }
}

export default class PlaylistOverview extends React.Component<IPlaylistOverviewProps, IPlaylistOverviewState> {
  private LOG_SOURCE: string = "PlaylistOverview";

  constructor(props) {
    super(props);
    this.state = new PlaylistOverviewState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistOverviewProps>, nextState: Readonly<IPlaylistOverviewState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IPlaylistOverviewProps> {
    try {
      return (
        <div className="plov">
          {this.props.playlistAssets && this.props.playlistAssets.length > 0 && this.props.playlistAssets.map((a, index) => {
            return (
              <AssetItem
                assetTitle={a.Title}
                assetDescription={a.Description}
                assetAudience={a.Audience}
                onClick={() => { this.props.assetClick(a.Id);}}
              />
            );
          })}
          {!this.props.playlistAssets &&
            <h3>{strings.PlaylistOverviewDesignMessage}</h3>
          }
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}