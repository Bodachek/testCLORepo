import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes } from "../../../common/models/Enums";

export interface ITechnologySubjectEditProps {
  techName: string;
  techSubject: string;
  visible: boolean;
  onVisibility: (techName: string, subTech: string, exists: boolean) => void;
}

export interface ITechnologySubjectEditState {
}

export class TechnologySubjectEditState implements ITechnologySubjectEditState {
  constructor() { }
}

export default class TechnologySubjectEdit extends React.Component<ITechnologySubjectEditProps, ITechnologySubjectEditState> {
  private LOG_SOURCE: string = "TechnologySubjectEdit";

  constructor(props) {
    super(props);
    this.state = new TechnologySubjectEditState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ITechnologySubjectEditProps>, nextState: Readonly<ITechnologySubjectEditState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ITechnologySubjectEditProps> {
    try {
      return (
        <div className="pl-edit-item">
          <span className="pl-edit-title">{this.props.techSubject}</span>
          <span className="pl-edit-actions">
            <Button buttonType={(this.props.visible) ? ButtonTypes.Show : ButtonTypes.Hide} onClick={() => { this.props.onVisibility(this.props.techName, this.props.techSubject, this.props.visible); }} disabled={false} />
          </span>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}