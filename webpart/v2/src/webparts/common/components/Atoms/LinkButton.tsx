import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IButtonType } from "../../models/Models";

import styles from "../../CustomLearningCommon.module.scss";

export interface ILinkButtonProps {
  buttonType: IButtonType;
  buttonLabel: string;
  onClick: () => void;
  disabled: boolean;
}

export interface ILinkButtonState {
}

export class LinkButtonState implements ILinkButtonState {
  constructor() { }
}

export default class LinkButton extends React.Component<ILinkButtonProps, ILinkButtonState> {
  private LOG_SOURCE: string = "LinkButton";

  constructor(props) {
    super(props);
    this.state = new LinkButtonState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ILinkButtonProps>, nextState: Readonly<ILinkButtonState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ILinkButtonProps> {
    try {
      return (
        <button className={`link-btn ${(this.props.disabled) ? "disabled" : ""}`} onClick={() => { if (!this.props.disabled) { this.props.onClick(); } }}>
          <span className="link-btn-icon" dangerouslySetInnerHTML={{ "__html": this.props.buttonType.SVG }} >
          </span>
          <span className="link-btn-label">{this.props.buttonLabel}</span>
        </button>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }

}