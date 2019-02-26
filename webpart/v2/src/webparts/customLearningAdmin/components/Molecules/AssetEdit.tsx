import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

import { PrimaryButton, DefaultButton } from "office-ui-fabric-react/lib/Button";
import DetailEdit from "../Atoms/DetailEdit";
import { IAsset, Asset, ICategory, ITechnology, IAudience, ISubCategory, IPlaylist } from "../../../common/models/Models";
import { CustomWebpartSource, Templates } from "../../../common/models/Enums";
import * as strings from "CustomLearningAdminWebPartStrings";

export interface IAssetEditProps {
  assets: IAsset[];
  categories: ICategory[];
  technologies: ITechnology[];
  levels: string[];
  audiences: IAudience[];
  assetId: string;
  selectedCategory: ICategory;
  selectedSubCategory: ISubCategory;
  detail?: IPlaylist | IAsset;
  cancel: () => void;
  save: (asset: IAsset) => Promise<boolean>;
  edit: boolean;
}

export interface IAssetEditState {
  asset: IAsset;
  assetChanged: boolean;
}

export class AssetEditState implements IAssetEditState {
  constructor(
    public asset: IAsset = null,
    public assetChanged: boolean = false
  ) { }
}

export default class AssetEdit extends React.Component<IAssetEditProps, IAssetEditState> {
  private LOG_SOURCE: string = "AssetEdit";
  private _reInit: boolean = false;

  constructor(props) {
    super(props);
    let asset: IAsset;
    if (this.props.assetId === "0") {
      asset = new Asset();
      //set the category and sub category based on parent for new assets
      asset.Category = this.props.selectedCategory.Name;
      asset.SubCategory = this.props.selectedSubCategory.Name;
      asset.Technology = this.props.detail.Technology;
    } else {
      asset = lodash.cloneDeep(lodash.find(this.props.assets, { Id: this.props.assetId }));
      if (!asset)
        asset = new Asset();
    }
    this.state = new AssetEditState(asset, (this.props.assetId === "0" ? true : false));
  }

  public init(): void {
    let asset: IAsset;
    if (this.props.assetId === "0") {
      asset = new Asset();
    } else {
      asset = lodash.cloneDeep(lodash.find(this.props.assets, { Id: this.props.assetId }));
      if (!asset)
        asset = new Asset();
    }
    this.setState({
      asset: asset,
      assetChanged: (this.props.assetId === "0" ? true : false)
    });
  }

  public shouldComponentUpdate(nextProps: Readonly<IAssetEditProps>, nextState: Readonly<IAssetEditState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    if (nextProps.assetId !== this.props.assetId)
      this._reInit = true;
    return true;
  }

  public componentDidUpdate() {
    if (this._reInit) {
      this._reInit = false;
      this.init();
    }
  }

  private updateAsset = (newAsset: IAsset) => {
    this.setState({
      asset: newAsset,
      assetChanged: true
    });
  }

  private saveAsset = async () => {
    if (this.props.save(this.state.asset)) {
      this.setState({ assetChanged: false });
    }
  }

  private assetValid(): boolean {
    let valid = true;
    if ((this.state.asset.Title.length < 1) ||
      (this.state.asset.Url.length < 1) ||
      (this.state.asset.Category.length < 1) ||
      (this.state.asset.SubCategory.length < 1) ||
      (this.state.asset.Level.length < 1) ||
      (this.state.asset.Audience.length < 1)
    )
      valid = false;

    return valid;
  }

  private assetHeader = () => {
    let header: string = "";
    try {
      if (this.state.asset.Id === "0") {
        header = strings.AssetEditCreateHeader;
      } else {
        if (this.state.asset.Source === CustomWebpartSource.Tenant) {
          header = `${strings.AssetEditManageHeader} ${this.state.asset.Title}`;
        } else {
          header = `${strings.AssetEditDetailsHeader} ${this.state.asset.Title}`;
        }
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (assetHeader)`, LogLevel.Error);
    }
    return header;
  }

  public render(): React.ReactElement<IAssetEditProps> {
    try {
      return (
        <div>
          <h3>{this.assetHeader()}</h3>
          <DetailEdit
            template={Templates.Asset}
            selectedCategory={this.props.selectedCategory}
            selectedSubCategory={this.props.selectedSubCategory}
            categories={this.props.categories}
            technologies={this.props.technologies}
            levels={this.props.levels}
            audiences={this.props.audiences}
            detail={this.state.asset}
            updateDetail={this.updateAsset}
            edit={this.props.edit}
          />
          <div className="adm-itemaction">
            <PrimaryButton text={strings.AssetEditSaveLabel} onClick={this.saveAsset} disabled={!this.state.assetChanged || !this.assetValid()} />
            <DefaultButton text={strings.AssetEditCancelLabel} onClick={this.props.cancel} />
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
