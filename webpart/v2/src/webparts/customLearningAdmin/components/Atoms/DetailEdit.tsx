import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { Dropdown, IDropdownOption } from 'office-ui-fabric-react/lib/Dropdown';
import { IPlaylist, IAsset, ICategory, ITechnology, IAudience, ISubCategory } from "../../../common/models/Models";
import { TextField } from 'office-ui-fabric-react/lib/TextField';
import * as strings from 'CustomLearningAdminWebPartStrings';
import { Label } from "office-ui-fabric-react/lib/Label";
import { Templates, CustomWebpartSource } from "../../../common/models/Enums";
import { CompoundButton, PrimaryButton } from "office-ui-fabric-react/lib/Button";
import { Spinner, SpinnerSize } from 'office-ui-fabric-react/lib/Spinner';
import { sp } from "@pnp/sp";

export interface IDetailEditProps {
  template: string;
  selectedCategory: ICategory;
  selectedSubCategory: ISubCategory;
  categories: ICategory[];
  technologies: ITechnology[];
  levels: string[];
  audiences: IAudience[];
  detail: IPlaylist | IAsset;
  edit: boolean;
  updateDetail: (detail: IPlaylist | IAsset) => void;
}

export interface IDetailEditState {
  selectedCategory: string;
  categoryDropdown: IDropdownOption[];
  selectedSubCategory: string;
  subCategoryDropdown: IDropdownOption[];
  technologyDropdown: IDropdownOption[];
  levelDropdown: IDropdownOption[];
  audienceDropdown: IDropdownOption[];
  enterUrl: boolean;
  startCreatePage: boolean;
  pageCreateError: string;
}

export class DetailEditState implements IDetailEditState {
  constructor(
    public selectedCategory: string = null,
    public categoryDropdown: IDropdownOption[] = [],
    public selectedSubCategory: string = null,
    public subCategoryDropdown: IDropdownOption[] = [],
    public technologyDropdown: IDropdownOption[] = [],
    public levelDropdown: IDropdownOption[] = [],
    public audienceDropdown: IDropdownOption[] = [],
    public enterUrl: boolean = false,
    public startCreatePage: boolean = false,
    public pageCreateError: string = ""
  ) { }
}

export default class DetailEdit extends React.Component<IDetailEditProps, IDetailEditState> {
  private LOG_SOURCE: string = "DetailEdit";

  constructor(props) {
    super(props);
    try {
      let categoryDropdown: IDropdownOption[] = this.props.categories.map((cat) => {
        return { key: cat.Id, text: cat.Name };
      });
      let selectedCategory: string = (this.props.selectedCategory) ? lodash.clone(this.props.selectedCategory.Id) : null;
      let selectedSubCategory: string = (this.props.selectedSubCategory) ? lodash.clone(this.props.selectedSubCategory.Id) : null;
      let subCategoryDropdown: IDropdownOption[] = [];
      if (this.props.detail.Id !== "0") {
        let selectedCategoryValue = lodash.find(this.props.categories, { Name: this.props.detail.Category });
        selectedCategory = (selectedCategoryValue) ? selectedCategoryValue.Id : null;
        if (selectedCategoryValue) {
          subCategoryDropdown = selectedCategoryValue.SubCategories.map((subcat) => {
            return { key: subcat.Id, text: subcat.Name };
          });
          let selectedSubCategoryValue = lodash.find(selectedCategoryValue.SubCategories, { Name: this.props.detail.SubCategory });
          selectedSubCategory = (selectedSubCategoryValue) ? selectedSubCategoryValue.Id : null;
        }
      } else {
        subCategoryDropdown = this.props.selectedCategory.SubCategories.map((subcat) => {
          return { key: subcat.Id, text: subcat.Name };
        });
      }
      let technologyDropdown: IDropdownOption[] = this.props.technologies.map((tech) => {
        return { key: tech.Name, text: tech.Name };
      });
      let levelDropdown: IDropdownOption[] = this.props.levels.map((level) => {
        return { key: level, text: level };
      });
      let audienceDropdown: IDropdownOption[] = this.props.audiences.map((aud) => {
        return { key: aud.Name, text: aud.Name };
      });
      this.state = new DetailEditState(
        selectedCategory,
        categoryDropdown,
        selectedSubCategory,
        subCategoryDropdown,
        technologyDropdown,
        levelDropdown,
        audienceDropdown);
      if (this.props.detail.Category === "" && selectedCategory != null) {
        let detail = lodash.cloneDeep(this.props.detail);
        detail.Category = lodash.find(categoryDropdown, { key: selectedCategory }).text;
        if (selectedSubCategory != null)
          detail.SubCategory = lodash.find(subCategoryDropdown, { key: selectedSubCategory }).text;
        (detail as IAsset).CatId = selectedSubCategory;
        this.props.updateDetail(detail);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (constructor)`, LogLevel.Error);
    }
  }

  public shouldComponentUpdate(nextProps: Readonly<IDetailEditProps>, nextState: Readonly<IDetailEditState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private textFieldChanged = (newValue: string, fieldName: string) => {
    try {
      let editDetail: IPlaylist | IAsset = lodash.clone(this.props.detail);
      editDetail[fieldName] = newValue;
      this.props.updateDetail(editDetail);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (textFieldChanged)`, LogLevel.Error);
    }
  }

  private dropdownChanged = (option: IDropdownOption, fieldName: string) => {
    try {
      let editDetail: IPlaylist | IAsset = lodash.clone(this.props.detail);
      editDetail[fieldName] = option.key.toString();
      this.props.updateDetail(editDetail);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (dropdownChanged)`, LogLevel.Error);
    }
  }

  private categoryChange = (option?: IDropdownOption, index?: number) => {
    try {
      let editDetail: IPlaylist | IAsset = lodash.clone(this.props.detail);
      let selectedCategoryValue = lodash.find(this.props.categories, { Name: option.text });
      editDetail.Category = selectedCategoryValue.Name;
      let selectedCategory: string = (selectedCategoryValue) ? selectedCategoryValue.Id : null;
      let subCategoryDropdown: IDropdownOption[] = selectedCategoryValue.SubCategories.map((subcat) => {
        return { key: subcat.Id, text: subcat.Name };
      });
      this.props.updateDetail(editDetail);
      this.setState({
        selectedCategory: selectedCategory,
        subCategoryDropdown: subCategoryDropdown
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (categoryChange)`, LogLevel.Error);
    }
  }

  private subCategoryChange = (option?: IDropdownOption, index?: number) => {
    try {
      let editDetail: IPlaylist | IAsset = lodash.clone(this.props.detail);
      editDetail.CatId = option.key.toString();
      editDetail.SubCategory = option.text;
      this.props.updateDetail(editDetail);
      this.setState({
        selectedSubCategory: option.key.toString()
      });
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (subCategoryChange)`, LogLevel.Error);
    }
  }

  private createPage = async () => {
    if (this.props.detail.Title.length < 1) return;
    try {
      let page = await sp.web.addClientSidePage(`${this.props.detail.Title}.aspx`);
      let pageItem = await page.getItem<{ Id: number, FileRef: string, PageLayoutType: string }>("Id", "FileRef", "PageLayoutType");
      //let pageItemResult = await pageItem.update({ PageLayoutType: "Home" });
      this.setState({ startCreatePage: false });
      this.textFieldChanged(`${document.location.origin}${pageItem.FileRef}`, "Url");
    } catch (err) {
      let errMsg = err.message;
      Logger.write(`${err} - ${this.LOG_SOURCE} (createPage)`, LogLevel.Error);
      this.setState({ startCreatePage: false, pageCreateError: `There was an error creating the page. '${errMsg}'` });
    }
  }

  private startCreatePage = () => {
    this.setState({ startCreatePage: true }, () => {
      this.createPage();
    });
  }

  private getEditRender = () => {
    let retVal = [];
    try {
      retVal = [
        <TextField
          value={this.props.detail.Title}
          label={strings.DetailEditTitle}
          required={true}
          onChanged={(newValue) => { this.textFieldChanged(newValue, "Title"); }}
          autoFocus={true}
        />,
        <TextField
          value={this.props.detail.Description}
          label={strings.DetailEditDescription}
          required={true}
          multiline
          rows={3}
          onChanged={(newValue) => { this.textFieldChanged(newValue, "Description"); }}
        />,
        <Dropdown
          label={strings.DetailEditCategory}
          options={this.state.categoryDropdown}
          selectedKey={this.state.selectedCategory}
          onChanged={this.categoryChange}
          required={true}
        />,
        <Dropdown
          label={strings.DetailEditSubCategory}
          options={this.state.subCategoryDropdown}
          selectedKey={this.state.selectedSubCategory}
          onChanged={this.subCategoryChange}
          required={true}
        />,
        <Dropdown
          label={strings.DetailEditTechnology}
          options={this.state.technologyDropdown}
          selectedKey={[this.props.detail.Technology]}
          onChanged={(option) => { this.dropdownChanged(option, "Technology"); }}
          required={false}
        />,
        <Dropdown
          label={strings.DetailEditLevel}
          options={this.state.levelDropdown}
          selectedKey={this.props.detail.Level}
          onChanged={(option) => { this.dropdownChanged(option, "Level"); }}
          required={true}
        />,
        <Dropdown
          label={strings.DetailEditAudience}
          options={this.state.audienceDropdown}
          selectedKey={this.props.detail.Audience}
          onChanged={(option) => { this.dropdownChanged(option, "Audience"); }}
          required={true}
        />
      ];
      if (this.props.template === Templates.Asset) {
        let assetUrlFields: Array<JSX.Element> = this.getAssetUrlFields();
        let index: number = 2;
        assetUrlFields.forEach((field) => {
          retVal.splice(index, 0, field);
          index++;
        });
        retVal.splice(1, 1);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getEditRender)`, LogLevel.Error);
    }
    return retVal;
  }

  private openAsset = () => {
    window.open((this.props.detail as IAsset).Url, '_blank');
  }

  private getAssetUrlFields(): Array<JSX.Element> {
    let retVal: Array<JSX.Element>;
    try {
      if (this.props.detail.Title.length < 1) {
        retVal = [<Label required={true}>Url: Awaiting title...</Label>];
      } else if ((this.props.detail as IAsset).Url.length > 0 || this.state.enterUrl) {
        retVal = [
          <TextField
            value={(this.props.detail as IAsset).Url}
            label={strings.DetailEditUrl}
            required={true}
            multiline
            rows={2}
            onChanged={(newValue) => { this.textFieldChanged(newValue, "Url"); }}
          />,
          <PrimaryButton text={strings.AssetEditOpenPage} onClick={this.openAsset} />];
      } else if ((this.props.detail as IAsset).Url.length < 1 && this.state.pageCreateError === "") {
        retVal = [<div>
          <Label required={true}>{strings.DetailEditUrl}</Label>
          <div>
            <CompoundButton primary={true} secondaryText={strings.DetailEditNewPageMessage} onClick={this.startCreatePage}>
              {strings.DetailEditNewPageButton}
            </CompoundButton>
            <CompoundButton primary={false} secondaryText={strings.DetailEditExistingPageMessage} onClick={() => { this.setState({ enterUrl: true }); }}>
              {strings.DetailEditExistingPageButton}
            </CompoundButton>
          </div>
        </div>];
      } else if (this.state.startCreatePage) {
        retVal = [<span>Creating page </span>,
        <Spinner size={SpinnerSize.medium} />];
      } else if (this.state.pageCreateError !== "") {
        retVal = [<Label required={true}>{strings.DetailEditUrl}</Label>,
        <Label className="ms-fontColor-redDark">{this.state.pageCreateError}</Label>,
        <PrimaryButton onClick={() => { this.setState({ pageCreateError: "" }); }}>Try Again</PrimaryButton>];
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getAssetUrlFields)`, LogLevel.Error);
    }
    return retVal;
  }

  private getNoEditRender = () => {
    let retVal = [];
    try {
      retVal = [
        <Label className="ms-fontWeight-semibold">{strings.DetailEditTitle}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Title}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditDescription}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Description}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditCategory}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Category}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditSubCategory}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.SubCategory}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditTechnology}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Technology}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditLevel}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Level}</p>,
        <Label className="ms-fontWeight-semibold">{strings.DetailEditAudience}</Label>,
        <p className="adm-fieldvalue">{this.props.detail.Audience}</p>
      ];
      if (this.props.template === Templates.Asset) {
        retVal.splice(4, 0, <Label className="ms-fontWeight-semibold">{strings.DetailEditUrl}</Label>);
        retVal.splice(5, 0, <p className="adm-fieldvalue">{(this.props.detail as IAsset).Url}</p>);
        if (this.props.detail.Source === CustomWebpartSource.Tenant)
          retVal.splice(6, 0, <PrimaryButton text={strings.AssetEditOpenPage} onClick={this.openAsset} />);
        retVal.splice(2, 1);
        retVal.splice(2, 1);
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (getNoEditRender)`, LogLevel.Error);
    }
    return retVal;
  }

  public render(): React.ReactElement<IDetailEditProps> {
    try {
      return (
        <div>
          {this.props.edit && this.getEditRender()}
          {!this.props.edit && this.getNoEditRender()}
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
