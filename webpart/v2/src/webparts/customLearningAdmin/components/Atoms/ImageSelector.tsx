import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IconButton } from 'office-ui-fabric-react/lib/Button';
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import * as strings from 'CustomLearningAdminWebPartStrings';
import { ButtonTypes } from "../../../common/models/Enums";
import Button from "../../../common/components/Atoms/Button";

export interface IImageSelectorProps {
  placeholderUrl: string;
  imageSource: string;
  imageWidth?: number;
  imageHeight?: number;
  disabled: boolean;
  setImageSource: (imageSource: string) => void;
}

export interface IImageSelectorState {
  newImageSource: string;
}

export class ImageSelectorState implements IImageSelectorState {
  constructor(
    public newImageSource: string = ""
  ) { }
}

export default class ImageSelector extends React.Component<IImageSelectorProps, IImageSelectorState> {
  private LOG_SOURCE: string = "ImageSelector";
  private textInput;

  constructor(props) {
    super(props);
    this.state = new ImageSelectorState((this.props.imageSource) ? this.props.imageSource : this.props.placeholderUrl);
    this.textInput = React.createRef();
  }

  public shouldComponentUpdate(nextProps: Readonly<IImageSelectorProps>, nextState: Readonly<IImageSelectorState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    if(!lodash.isEqual(nextProps.imageSource, this.props.imageSource))
      return false;
    return true;
  }

  public render(): React.ReactElement<IImageSelectorProps> {
    try {
      return (
        <div className='adm-itemimage'>
          <img
            src={this.state.newImageSource}
            alt={strings.ImageSelectorImageAlt}
            className='adm-itemimage'
          />
          {!this.props.disabled &&
            <div>
              <TextField
                label={strings.ImageSelectorLabel}
                defaultValue={this.props.imageSource}
                required={true}
                onChanged={(newValue: string) => { this.props.setImageSource(newValue); }}
                onBlur={() => this.setState({newImageSource: this.props.imageSource}) }
                placeholder={strings.ImageSelectorUrlPlaceholder}
                multiline
                rows={2}
                ref={this.textInput}
              />              
            </div>
          }
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}