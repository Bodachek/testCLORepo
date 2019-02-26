import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import SearchResults from "../../../common/components/Molecules/SearchResults";
import { ISearchResult } from "../../../common/models/Models";
import { SearchResultHeaderFilters, SearchResultView } from "../../../common/models/Enums";

export interface IAssetSearchPanelProps {
  searchResults: ISearchResult[];
  loadSearchResult: (subcategoryId: string, playlistId: string, assetId: string) => void;
}

export interface IAssetSearchPanelState { }

export class AssetSearchPanelState implements IAssetSearchPanelState {
  constructor() { }
}

export default class AssetSearchPanel extends React.Component<IAssetSearchPanelProps, IAssetSearchPanelState> {
  private LOG_SOURCE: string = "AssetSearchPanel";

  constructor(props) {
    super(props);
    this.state = new AssetSearchPanelState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IAssetSearchPanelProps>, nextState: Readonly<IAssetSearchPanelState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IAssetSearchPanelProps> {
    try {
      if (!this.props.searchResults || this.props.searchResults.length < 1) return null;
      return (
        <div className="headerpanel blablala">
          <div className="fbrow">
            <SearchResults
              resultView={SearchResultView.Minimal}
              headerItems={[SearchResultHeaderFilters.Assets]}
              searchValue={null}
              searchResults={this.props.searchResults}
              loadSeachResult={this.props.loadSearchResult}
            />
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (assetHeader)`, LogLevel.Error);
      return null;
    }
  }
}
