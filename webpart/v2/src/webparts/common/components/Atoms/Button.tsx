import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import * as lodash from "lodash";
import "@pnp/polyfill-ie11";

import { IButtonType } from "../../models/Models";

export interface IButtonProps {
  buttonType: IButtonType;
  disabled: boolean;
  onClick: () => void;
  selected?: boolean;
  className?: string;
  title?: string;
}

export interface IButtonState {
}

export class ButtonState implements IButtonState {
  constructor() { }
}

export default class Button extends React.Component<IButtonProps, IButtonState> {
  private LOG_SOURCE: string = "Button";

  constructor(props) {
    super(props);
    this.state = new ButtonState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IButtonProps>, nextState: Readonly<IButtonState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IButtonProps> {
    try {
      return (
        <button title={this.props.title} className={`iconbutton ${(this.props.className) ? this.props.className : ""} ${(this.props.disabled) ? "disabled" : ""} ${(this.props.selected) ? "selected" : ""}`} onClick={() => { if (!this.props.disabled) { this.props.onClick(); } }}>
          <span className="iconbutton-img" dangerouslySetInnerHTML={{ "__html": this.props.buttonType.SVG }} >
          </span>
        </button>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}