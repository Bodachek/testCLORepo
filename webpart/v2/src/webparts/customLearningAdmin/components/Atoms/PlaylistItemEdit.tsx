import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes } from "../../../common/models/Enums";
import * as strings from "CustomLearningAdminWebPartStrings";

export interface IPlaylistItemEditProps {
  playlistId: string;
  playlistTitle: string;
  playlistVisible: boolean;
  playlistEditable: boolean;
  onVisible: (playlistId: string, exists: boolean) => void;
  onEdit: () => void;
  onClick: () => void;
  onMoveDown: () => void;
  onMoveUp: () => void;
  onDelete: () => void;
}

export interface IPlaylistItemEditState {
}

export class PlaylistItemEditState implements IPlaylistItemEditState {
  constructor() { }
}

export default class PlaylistItemEdit extends React.Component<IPlaylistItemEditProps, IPlaylistItemEditState> {
  private LOG_SOURCE: string = "PlaylistItemEdit";

  constructor(props) {
    super(props);
    this.state = new PlaylistItemEditState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistItemEditProps>, nextState: Readonly<IPlaylistItemEditState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IPlaylistItemEditProps> {
    try {
      // Setting title for aria-lable and title
      let title = this.props.playlistTitle + (this.props.playlistEditable ? " - Custom Playlist" : "");

      return (
        <div className={`pl-edit-item ${(this.props.playlistEditable) ? "custom" : ""}`} title={title} aria-title={title}>
          <span className="pl-edit-title" onClick={this.props.onClick}>{this.props.playlistTitle}</span>
          <span className="pl-edit-actions">
            {this.props.playlistEditable &&
              <Button title={strings.PlaylistItemEditPlaylistDelete} buttonType={ButtonTypes.Close} onClick={this.props.onDelete} disabled={false} />
            }
            {!this.props.playlistEditable &&
              <Button title={`${(this.props.playlistVisible) ? strings.Hide : strings.Show} ${strings.PlaylistItemEditPlaylistHeadingLabel}`} buttonType={(this.props.playlistVisible) ? ButtonTypes.Show : ButtonTypes.Hide} onClick={() => { this.props.onVisible(this.props.playlistId, this.props.playlistVisible); }} disabled={false} />
            }
          </span>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
