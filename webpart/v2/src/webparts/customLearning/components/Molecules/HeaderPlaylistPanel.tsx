import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

import { IAsset } from "../../../common/models/Models";
import PlaylistAssets from "../Atoms/PlaylistAssets";
import { ButtonTypes, Roles } from "../../../common/models/Enums";
import LinkButton from "../../../common/components/Atoms/LinkButton";
import * as strings from "CustomLearningWebPartStrings";

export interface IHeaderPlaylistPanelProps {
  userSecurity: string;
  panelOpen: string;
  playlistTitle: string;
  assets: IAsset[];
  assetClick: (assetId: string) => void;
  currentAssetId: string;
  adminPlaylistClick: () => void;
}

export interface IHeaderPlaylistPanelState {
}

export class HeaderPlaylistPanelState implements IHeaderPlaylistPanelState {
  constructor() { }
}

export default class HeaderPlaylistPanel extends React.Component<IHeaderPlaylistPanelProps, IHeaderPlaylistPanelState> {
  private LOG_SOURCE: string = "HeaderPlaylistPanel";

  constructor(props) {
    super(props);
    this.state = new HeaderPlaylistPanelState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IHeaderPlaylistPanelProps>, nextState: Readonly<IHeaderPlaylistPanelState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IHeaderPlaylistPanelProps> {
    try {
      return (
        <div className={`headerpanel fbrow ${(this.props.panelOpen.length > 0) ? "show" : ""}`}>
          <div className="headerpanel-left">
            {this.props.assets && this.props.assets.length > 0 &&
              <div>
                <h2 className="headerpanel-heading">{strings.HeaderPlaylistPanelCurrentPlaylistLabel}{this.props.playlistTitle}</h2>
                <PlaylistAssets
                  playlistAssets={this.props.assets}
                  currentAssetId={this.props.currentAssetId}
                  assetClick={this.props.assetClick}
                />
              </div>
            }
          </div>
          <div className="headerpanel-right">
            {(this.props.userSecurity !== Roles.Visitors) && [
              <h2 className="headerpanel-heading">{strings.HeaderPlaylistPanelAdminHeader}</h2>,
              <ul className="adminlist">
                <li className="adminlist-item">
                  <LinkButton buttonType={ButtonTypes.Gear} buttonLabel={strings.AdministerPlaylist} onClick={this.props.adminPlaylistClick} disabled={false} />
                </li>
              </ul>
            ]}
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}