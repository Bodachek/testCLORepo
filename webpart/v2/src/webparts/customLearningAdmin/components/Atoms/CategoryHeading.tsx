import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes } from "../../../common/models/Enums";
import { TextField } from "office-ui-fabric-react/lib/TextField";
import * as strings from "CustomLearningAdminWebPartStrings";
import { IconButton } from "office-ui-fabric-react/lib/Button";

export interface ICategoryHeadingProps {
  heading: string;
  id: string;
  playlistCount: number;
  editable: boolean;
  visible: boolean;
  saveSubCategory: (Id: string, newValue: string) => void;
  addPlaylist?: () => void;
  onVisibility: (subCategory: string, exists: boolean) => void;
  onDelete: () => void;
}

export interface ICategoryHeadingState {
  edit: boolean;
  editHeadingValue: string;
}

export class CategoryHeadingState implements ICategoryHeadingState {
  constructor(
    public edit: boolean = false,
    public editHeadingValue: string = ""
  ) { }
}

export default class CategoryHeading extends React.Component<ICategoryHeadingProps, ICategoryHeadingState> {
  private LOG_SOURCE: string = "CategoryHeading";

  constructor(props) {
    super(props);
    this.state = new CategoryHeadingState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ICategoryHeadingProps>, nextState: Readonly<ICategoryHeadingState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private onUpdate = () => {
    if (this.state.editHeadingValue.length < 1) return;
    this.setState({
      edit: false,
      editHeadingValue: ""
    });
    this.props.saveSubCategory(this.props.id, this.state.editHeadingValue);
  }

  private onDelete = () => {
    this.setState({
      edit: false,
      editHeadingValue: ""
    });
    this.props.onDelete();
  }

  private renderHeading = () => {
    try {
      if (this.state.edit) {
        return (
          <div className="category-edit">
            <TextField
              required={true}
              value={this.state.editHeadingValue}
              onChanged={(newValue) => { this.setState({ editHeadingValue: newValue }); }}
              autoFocus={true}
            />
          </div>);
      } else {
        return (
          <h3 className='category-heading'>
            {this.props.heading}
          </h3>);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (renderHeading)`, LogLevel.Error);
      return null;
    }
  }

  private renderButtons = () => {
    let retVal = [];
    try {
      if (this.state.edit) {
        retVal.push(
          <IconButton
            iconProps={{ iconName: 'Save' }}
            title={strings.SaveButton}
            ariaLabel={strings.SaveButton}
            onClick={this.onUpdate}
            className="iconbutton"
          />);
        retVal.push(
          <IconButton
            iconProps={{ iconName: 'Delete' }}
            title={strings.DeleteButton}
            ariaLabel={strings.DeleteButton}
            onClick={this.onDelete}
            disabled={this.props.playlistCount > 0}
            className="iconbutton"
          />);
        retVal.push(
          <IconButton
            iconProps={{ iconName: 'ChromeClose' }}
            title={strings.CancelButton}
            ariaLabel={strings.CancelButton}
            onClick={() => { this.setState({ edit: false }); }}
            className="iconbutton" />
        );
      } else {
        retVal.push(<Button
          title={`${(this.props.id === "0") ? strings.Add : strings.Edit} ${strings.SubcategoryHeadingLabel}`}
          buttonType={(this.props.id === "0") ? ButtonTypes.Add : ButtonTypes.Edit}
          onClick={() => { this.setState({ edit: true, editHeadingValue: (this.props.id === "0") ? "" : this.props.heading }); }}
          disabled={!this.props.editable}
          className="iconbutton"
        />);
        retVal.push(
          <Button
            title={`${(this.props.visible) ? strings.Hide : strings.Show} ${strings.CategoryHeadingLabel}`}
            buttonType={(this.props.visible) ? ButtonTypes.Show : ButtonTypes.Hide}
            onClick={() => { this.props.onVisibility(this.props.id, this.props.visible); }}
            disabled={(this.props.id === "0")}
            className="iconbutton"
          />);
        if (this.props.addPlaylist) {
          retVal.push(
            <Button
              title={strings.CategoryHeadingAddPlaylistToSubcategory}
              buttonType={ButtonTypes.Add}
              disabled={false}
              selected={false}
              onClick={this.props.addPlaylist}
              className="iconbutton"
            />);
        }
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (renderButtons)`, LogLevel.Error);
    }
    return retVal;
  }

  public render(): React.ReactElement<ICategoryHeadingProps> {
    try {
      return (
        <div className="admpl-heading">
          {this.renderHeading()}
          <span className="admpl-heading-edit">
            {this.renderButtons()}
          </span>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
