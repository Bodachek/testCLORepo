import * as React from 'react';
import "@pnp/polyfill-ie11";
import * as lodash from 'lodash';
import { ICategory, ISubCategory, IPlaylist, IFilter, IFilterValue } from '../../../common/models/Models';
import FilterPanel from "../Atoms/FilterPanel";
import SubCategoryList from '../Organisms/SubCategoryList';
import { FilterTypes } from '../../../common/models/Enums';

export interface ISubCategoriesProps {
  parent: ICategory | ISubCategory;
  template: string;
  detail: ISubCategory[] | IPlaylist[];
  filter: IFilter;
  filterValues: IFilterValue[];
  selectItem: (template: string, templateId: string) => void;
  setFilter: (filterValue: IFilterValue) => void;
}

export interface ISubCategoriesState {
}

export class SubCategoriesState implements ISubCategoriesState {
  constructor() { }
}

export default class SubCategories extends React.Component<ISubCategoriesProps, ISubCategoriesState> {
  constructor(props) {
    super(props);
    this.state = new SubCategoriesState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ISubCategoriesProps>, nextState: Readonly<ISubCategoriesState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ISubCategoriesProps> {
    let filterCounts = lodash.countBy(this.props.filterValues, "Key");
    return (
      <div>
        <h2>{this.props.parent.Name}</h2>
        {(filterCounts[FilterTypes.Audience] > 1 || filterCounts[FilterTypes.Level] > 1) &&
          <FilterPanel
            filter={this.props.filter}
            filterValues={this.props.filterValues}
            setFilter={this.props.setFilter}
          />
        }
        <SubCategoryList
          detail={this.props.detail}
          template={this.props.template}
          selectItem={this.props.selectItem}
        />
      </div>
    );
  }
}
