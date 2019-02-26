import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

export interface ISubCategoryItemProps {
  imageSource: string;
  title: string;
  description: string;
  audience: string;
  onClick: () => void;
}

export interface ISubCategoryItemState {
}

export class SubCategoryItemState implements ISubCategoryItemState {
  constructor() { }
}

export default class SubCategoryItem extends React.Component<ISubCategoryItemProps, ISubCategoryItemState> {
  private LOG_SOURCE: string = "SubCategoryItem";

  constructor(props) {
    super(props);
    this.state = new SubCategoryItemState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ISubCategoryItemProps>, nextState: Readonly<ISubCategoryItemState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ISubCategoryItemProps> {
    return (
      <div className="plov-item" onClick={this.props.onClick}>
        <div className="plov-img">
          <img src={this.props.imageSource} /> 
        </div>
        <div className="plov-desc">
          <h3 className="plov-title">{this.props.title}</h3>
          {/*<p className="plov-short">{this.props.description}</p>*/}
          {this.props.audience && this.props.audience.length > 0 &&
            <div className="plov-audience">For {this.props.audience}</div>
          }
        </div>
      </div>
    );
  }
}
