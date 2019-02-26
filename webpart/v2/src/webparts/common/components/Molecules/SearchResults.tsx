import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { PivotItem } from 'office-ui-fabric-react/lib/Pivot';
import { ISearchResult } from "../../models/Models";
import SearchResultItem from "../Atoms/SearchResultItem";
import { Templates, SearchResultHeaderFilters } from "../../models/Enums";
import SearchResultHeader from "../Atoms/SearchResultHeader";

export interface ISearchResultsProps {
  headerItems: string[];
  resultView: string;
  searchValue: string;
  searchResults: ISearchResult[];
  loadSeachResult: (subcategoryId: string, playlistId: string, assetId: string) => void;
}

export interface ISearchResultsState {
  filter: string;
}

export class SearchResultsState implements ISearchResultsState {
  constructor(
    public filter: string = "All"
  ) { }
}

export default class SearchResults extends React.Component<ISearchResultsProps, ISearchResultsState> {
  private LOG_SOURCE: string = "SearchResults";

  constructor(props) {
    super(props);
    this.state = new SearchResultsState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ISearchResultsProps>, nextState: Readonly<ISearchResultsState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private changeFilter = (newFilter: PivotItem) => {
    this.setState({
      filter: newFilter.props.linkText
    });
  }

  public render(): React.ReactElement<ISearchResultsProps> {
    try {
      let results = this.props.searchResults;
      if (this.state.filter !== SearchResultHeaderFilters.All) {
        if (this.state.filter == SearchResultHeaderFilters.Assets) {
          results = lodash.filter(results, { Type: Templates.Asset });
        } else if (this.state.filter == SearchResultHeaderFilters.Playlists) {
          results = lodash.filter(results, { Type: Templates.Playlist });
        }
      }
      return (
        <div>
          <SearchResultHeader
            headerItems={this.props.headerItems}
            searchValue={this.props.searchValue}
            selectTab={this.changeFilter}
          />
          {results && results.length > 0 && results.map((result) => {
            return (
              <SearchResultItem
                resultView={this.props.resultView}
                result={result}
                loadSeachResult={this.props.loadSeachResult}
              />
            );
          })}
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }

}