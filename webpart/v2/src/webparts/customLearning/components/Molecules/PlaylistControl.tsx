import * as React from "react";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes } from "../../../common/models/Enums";

export interface IPlaylistControlProps {
  playlistTitle: string;
  onAdvance: () => void;
  disableAdvance: boolean;
  onBack: () => void;
  disableBack: boolean;
}

export interface IPlaylistControlState {
}

export class PlaylistControlState implements IPlaylistControlState {
  constructor() { }
}

export default class PlaylistControl extends React.Component<IPlaylistControlProps, IPlaylistControlState> {
  private LOG_SOURCE: string = "PlaylistControl";

  constructor(props) {
    super(props);
    this.state = new PlaylistControlState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistControlProps>, nextState: Readonly<IPlaylistControlState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IPlaylistControlProps> {
    return (
      <div className="playerctrl">
        <span className="playerctrl-prev">
          <Button buttonType={ButtonTypes.ArrowLeft} onClick={this.props.onBack} disabled={this.props.disableBack} title="Previous chapter" />
        </span>
        <span className="playerctrl-title">
          {this.props.playlistTitle}
        </span>
        <span className="playerctrl-next">
          <Button buttonType={ButtonTypes.ArrowRight} onClick={this.props.onAdvance} disabled={this.props.disableAdvance} title="Next chapter" />
        </span>
      </div>
    );
  }
}
