import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

export interface ITextButtonProps {
  label: string;
  selected: boolean;
  onClick: () => void;
}

export interface ITextButtonState {
}

export class TextButtonState implements ITextButtonState {
  constructor() { }
}

export default class TextButton extends React.Component<ITextButtonProps, ITextButtonState> {
  private LOG_SOURCE: string = "TextButton";

  constructor(props) {
    super(props);
    this.state = new TextButtonState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ITextButtonProps>, nextState: Readonly<ITextButtonState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ITextButtonProps> {
    try {
      return (
        <button className={`btn-small ${(this.props.selected) ? "selected" : ""}`} onClick={() => { this.props.onClick(); }}>{this.props.label}</button>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}