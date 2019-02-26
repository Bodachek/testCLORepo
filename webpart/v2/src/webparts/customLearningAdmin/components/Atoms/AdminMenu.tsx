import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";

import { Pivot, PivotItem } from 'office-ui-fabric-react/lib/Pivot';
import * as strings from "CustomLearningAdminWebPartStrings";

export interface IAdminMenuProps {
  selectTab: (tab: PivotItem) => void;
}

export interface IAdminMenuState {
}

export class AdminMenuState implements IAdminMenuState {
  constructor() { }
}

export default class AdminMenu extends React.Component<IAdminMenuProps, IAdminMenuState> {
  private LOG_SOURCE: string = "AdminMenu";

  constructor(props) {
    super(props);
    this.state = new AdminMenuState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IAdminMenuProps>, nextState: Readonly<IAdminMenuState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public render(): React.ReactElement<IAdminMenuProps> {
    try {
      return (
        <Pivot
          onLinkClick={this.props.selectTab}
          className="adm-header-nav"
        >
          <PivotItem linkText={strings.AdminMenuCategoryLabel} />
          <PivotItem linkText={strings.AdminMenuTechnologyLabel} />
        </Pivot>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
