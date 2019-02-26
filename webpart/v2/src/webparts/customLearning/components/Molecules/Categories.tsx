import * as React from 'react';
import { Logger, LogLevel } from '@pnp/logging';
import "@pnp/polyfill-ie11";
import * as lodash from 'lodash';
import { ICategory } from '../../../common/models/Models';
import CategoryItem from "../Atoms/CategoryItem";

export interface ICategoriesProps {
  detail: ICategory[];
  selectItem: (template: string, templateId: string) => void;
}

export interface ICategoriesState {
}

export class CategoriesState implements ICategoriesState {
  constructor() { }
}

export default class Categories extends React.Component<ICategoriesProps, ICategoriesState> {
  private LOG_SOURCE = "Categories";

  constructor(props) {
    super(props);
    this.state = new CategoriesState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ICategoriesProps>, nextState: Readonly<ICategoriesState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<ICategoriesProps> {
    try {
      return (
        <div>
          <div>
            {this.props.detail && this.props.detail.length > 0 && this.props.detail.map((category) => {
              return (
                <div>
                  <h2>{category.Name}</h2>
                  <div className="category-overview">
                    {category.SubCategories && category.SubCategories.length > 0 && category.SubCategories.map((subcategory) => {
                      if (subcategory.Count && subcategory.Count > 0){
                        return (
                          <CategoryItem
                            subcategoryId={subcategory.Id}
                            subcategoryImage={subcategory.Image}
                            subcategoryName={subcategory.Name}
                            selectItem={this.props.selectItem}
                          />
                        );
                      }
                    })}
                  </div>
                </div>
              );
            })
            }
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
