import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IPlaylist, ISubCategory, ISearchResult } from "../../models/Models";
import { Templates, SearchResultView } from "../../models/Enums";
import * as strings from "CustomLearningWebPartStrings";

export interface ISearchResultItemProps {
  result: ISearchResult;
  resultView: string;
  loadSeachResult: (subcategoryId: string, playlistId: string, assetId: string) => void;
}

export interface ISearchResultItemState {
}

export class SearchResultItemState implements ISearchResultItemState {
  constructor() { }
}

export default class SearchResultItem extends React.Component<ISearchResultItemProps, ISearchResultItemState> {
  private LOG_SOURCE: string = "SearchResultItem";

  constructor(props) {
    super(props);
    this.state = new SearchResultItemState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ISearchResultItemProps>, nextState: Readonly<ISearchResultItemState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ISearchResultItemProps> {
    try {
      return (
        <div>
          {(this.props.result.Type === Templates.Asset) &&
            <div className="plov-item plov-noimg">
              <div className="plov-desc">
                <h3 className="plov-title" onClick={() => { this.props.loadSeachResult(null, this.props.result.Parent ? this.props.result.Parent.Id : null, this.props.result.Result.Id); }}>{this.props.result.Result.Title}</h3>
                {this.props.resultView !== SearchResultView.Minimal &&
                  <p className="plov-short">{this.props.result.Result.Description}</p>
                }
                {this.props.resultView === SearchResultView.Full &&
                  <div className="plov-audience">
                    <span>{strings.SearchResultItemPlayListLabel}</span>
                    <span className="plov-title" onClick={() => { this.props.loadSeachResult(null, this.props.result.Parent.Id, null); }}>{(this.props.result.Parent as IPlaylist).Title}</span>
                  </div>
                }
              </div>
            </div>
          }
          {(this.props.result.Type === Templates.Playlist) &&
            <div className="plov-item plov-noimg">
              <div className="plov-desc">
                <h3 className="plov-title" onClick={() => { this.props.loadSeachResult(this.props.result.Parent ? this.props.result.Parent.Id : null, this.props.result.Result.Id, null); }}>{this.props.result.Result.Title}</h3>
                {this.props.resultView !== SearchResultView.Minimal &&
                  <p className="plov-short">{this.props.result.Result.Description}</p>
                }
                {this.props.resultView === SearchResultView.Full &&
                  <div className="plov-audience">
                    <span>{strings.SearchResultItemCategoryLabel}</span>
                    <span className="plov-title" onClick={() => { this.props.loadSeachResult(this.props.result.Parent.Id, null, null); }}>{(this.props.result.Parent as ISubCategory).Name}</span>
                  </div>
                }
              </div>
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