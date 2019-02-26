import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IAsset } from "../../../common/models/Models";

export interface IPlaylistAssetsProps {
  playlistAssets: IAsset[];
  currentAssetId: string;
  assetClick: (assetId: string) => void;
}

export interface IPlaylistAssetsState {
}

export class PlaylistAssetsState implements IPlaylistAssetsState {
  constructor() { }
}

export default class PlaylistAssets extends React.Component<IPlaylistAssetsProps, IPlaylistAssetsState> {
  private LOG_SOURCE: string = "PlaylistAssets";

  constructor(props) {
    super(props);
    this.state = new PlaylistAssetsState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistAssetsProps>, nextState: Readonly<IPlaylistAssetsState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IPlaylistAssetsProps> {
    try {
      return (
        <ul className="pl-nav">
          {this.props.playlistAssets && this.props.playlistAssets.length > 0 && this.props.playlistAssets.map((a, index) => {
            return (
              <li className={`pl-item ${(a.Id === this.props.currentAssetId) ? "selected" : ""}`} onClick={() => { this.props.assetClick(a.Id); }}>
                <span className="pl-step">{(index + 1).toString()}.</span>
                {a.Title}
              </li>
            );
          })}
        </ul>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
