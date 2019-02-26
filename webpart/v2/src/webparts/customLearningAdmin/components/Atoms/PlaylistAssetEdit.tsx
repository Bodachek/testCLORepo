import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IAsset } from "../../../common/models/Models";
import { CommandBar } from "office-ui-fabric-react/lib/CommandBar";
import * as strings from "CustomLearningAdminWebPartStrings";
import { IContextualMenuItem } from "office-ui-fabric-react/lib/ContextualMenu";

export interface IPlaylistAssetEditProps {
  assetIndex: number;
  assetTotal: number;
  asset: IAsset;
  editDisabled: boolean;
  allDisabled: boolean;
  editAssetId: string;
  editAsset: boolean;
  edit: () => void;
  moveUp: () => void;
  moveDown: () => void;
  remove: () => void;
  select: () => void;
}

export interface IPlaylistAssetEditState {
}

export class PlaylistAssetEditState implements IPlaylistAssetEditState {
  constructor() { }
}

export default class PlaylistAssetEdit extends React.Component<IPlaylistAssetEditProps, IPlaylistAssetEditState> {
  private LOG_SOURCE: string = "PlaylistAssetEdit";

  constructor(props) {
    super(props);
    this.state = new PlaylistAssetEditState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IPlaylistAssetEditProps>, nextState: Readonly<IPlaylistAssetEditState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private getAssetCommandFarItems = (): IContextualMenuItem[] => {
    let retVal: IContextualMenuItem[] = [];
    try {
      if (!this.props.allDisabled) {
        retVal.push({
          key: 'moveUp',
          name: strings.MoveUpButton,
          iconOnly: true,
          iconProps: {
            iconName: 'ChevronUp'
          },
          disabled: (this.props.assetIndex === 0),
          onClick: () => this.props.moveUp()
        });
        retVal.push({
          key: 'moveDown',
          name: strings.MoveDownButton,
          iconOnly: true,
          iconProps: {
            iconName: 'ChevronDown'
          },
          disabled: (this.props.assetIndex === this.props.assetTotal),
          onClick: () => this.props.moveDown()
        });
        retVal.push({
          key: 'remove',
          name: strings.PlaylistRemove,
          iconOnly: true,
          iconProps: {
            iconName: 'ChromeClose'
          },
          onClick: () => this.props.remove()
        });
        retVal.push({
          key: 'edit',
          name: strings.EditButton,
          iconOnly: true,
          iconProps: {
            iconName: 'Edit'
          },
          disabled: (this.props.editDisabled || this.props.editAsset),
          onClick: () => this.props.edit()
        });
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getAssetCommandFarItems)`, LogLevel.Error);
    }
    return retVal;
  }

  public render(): React.ReactElement<IPlaylistAssetEditProps> {
    try {
      return (
        <CommandBar
          items={[{
            key: 'title',
            name: `${strings.StepButton} ${this.props.assetIndex + 1}: ${this.props.asset.Title}`,
            onClick: () => this.props.select()
          }]}
          farItems={this.getAssetCommandFarItems()}
        />
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}