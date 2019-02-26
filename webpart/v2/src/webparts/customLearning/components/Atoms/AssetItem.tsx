import * as React from "react";
import "@pnp/polyfill-ie11";
import { Logger, LogLevel } from '@pnp/logging';
import * as lodash from "lodash";

export interface IAssetItemProps {
  assetTitle: string;
  assetDescription: string;
  assetAudience: string;
  onClick: () => void;
}

export interface IAssetItemState {
}

export class AssetItemState implements IAssetItemState {
  constructor() { }
}

export default class AssetItem extends React.Component<IAssetItemProps, IAssetItemState> {
  private LOG_SOURCE: string = "AssetItem";

  constructor(props) {
    super(props);
    this.state = new AssetItemState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IAssetItemProps>, nextState: Readonly<IAssetItemState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IAssetItemProps> {
    try {
      return (
        <div className="plov-item plov-noimg" onClick={this.props.onClick}>
          <div className="plov-desc">
            <h3 className="plov-title">{this.props.assetTitle}</h3>
            <p className="plov-short">{this.props.assetDescription}</p>
            {this.props.assetAudience && this.props.assetAudience.length > 0 &&
              <div className="plov-audience">For {this.props.assetAudience}</div>
            }
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
