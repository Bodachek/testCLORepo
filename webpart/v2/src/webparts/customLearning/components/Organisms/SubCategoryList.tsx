import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { ISubCategory, IPlaylist } from '../../../common/models/Models';
import { Templates } from "../../../common/models/Enums";
import SubCategoryItem from "../Molecules/SubcategoryItem";

export interface ISubCategoryListProps {
  detail: ISubCategory[] | IPlaylist[];
  template: string;
  selectItem: (template: string, templateId: string) => void;
}

export interface ISubCategoryListState {
}

export class SubCategoryListState implements ISubCategoryListState {
  constructor() { }
}

export default class SubCategoryList extends React.Component<ISubCategoryListProps, ISubCategoryListState> {
  private LOG_SOURCE: string = "SubCategoryList";

  constructor(props) {
    super(props);
    this.state = new SubCategoryListState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ISubCategoryListProps>, nextState: Readonly<ISubCategoryListState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ISubCategoryListProps> {
    try {
      return (
        <div className="plov">
          {this.props.detail && this.props.detail.length > 0 && (this.props.template == Templates.SubCategory) && (this.props.detail as ISubCategory[]).map((subcategory) => {
            return (
              <SubCategoryItem
                imageSource={(subcategory.Image.length > 0) ? subcategory.Image : null}
                title={subcategory.Name}
                description=""
                audience=""
                onClick={() => this.props.selectItem(Templates.SubCategory, subcategory.Id)} />
            );
          })
          }
          {this.props.detail && this.props.detail.length > 0 && (this.props.template == Templates.Playlists) && (this.props.detail as IPlaylist[]).map((playlist) => {
            return (
              <SubCategoryItem
                imageSource={(playlist.Image.length > 0) ? playlist.Image : null}
                title={playlist.Title}
                description={playlist.Description}
                audience={playlist.Audience}
                onClick={() => this.props.selectItem(Templates.Playlist, playlist.Id)} />
            );
          })
          }
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
