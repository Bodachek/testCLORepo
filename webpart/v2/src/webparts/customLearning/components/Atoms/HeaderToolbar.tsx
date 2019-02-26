import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { IHistoryItem } from "../../../common/models/Models";
import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes, Templates, Roles, WebpartMode } from "../../../common/models/Enums";
import { Breadcrumb, IBreadcrumbItem } from 'office-ui-fabric-react/lib/Breadcrumb';
import { Label } from "office-ui-fabric-react/lib/Label";
import styles from "../../../common/CustomLearningCommon.module.scss";

export interface IHeaderToolbarProps {
  userSecurity: string;
  template: string;
  history: IHistoryItem[];
  historyClick: (template: string, templateId: string, nav: boolean) => void;
  buttonClick: (buttonType: string) => void;
  panelOpen: string;
  webpartMode: string;
  webpartTitle: string;
}

export interface IHeaderToolbarState {
}

export class HeaderToolbarState implements IHeaderToolbarState {
  constructor() { }
}

export default class HeaderToolbar extends React.Component<IHeaderToolbarProps, IHeaderToolbarState> {
  private LOG_SOURCE: string = "HeaderToolbar";

  constructor(props) {
    super(props);
    this.state = new HeaderToolbarState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IHeaderToolbarProps>, nextState: Readonly<IHeaderToolbarState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private onBreadcrumbItemClicked = (event, item: IBreadcrumbItem): void => {
    try {
      let history = lodash.find(this.props.history, { Id: item.key });
      this.props.historyClick(history.Template, history.Id, true);
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (onBreadcrumbItemClicked)`, LogLevel.Error);
    }
  }

  public render(): React.ReactElement<IHeaderToolbarProps> {
    try {
      let breadcrumbItems: IBreadcrumbItem[] = [];
      if (this.props.history && this.props.history.length > 0) {
        breadcrumbItems = this.props.history.map((history) => {
          return { text: history.Name, key: history.Id, onClick: this.onBreadcrumbItemClicked };
        });
      }
      return (
        <div className="header-toolbar">
          <div className="header-breadcrumb">

            {(this.props.webpartMode !== WebpartMode.contentonly) &&
            //This works fine with static data but not with dynamic data.
              <Breadcrumb
                onReduceData={(data) => undefined}
                maxDisplayedItems={4}
                items={breadcrumbItems}
              />
            }
            {(this.props.webpartMode === WebpartMode.contentonly) && (this.props.webpartTitle && this.props.webpartTitle.length > 0) &&
              <h2 className={styles.title}>{this.props.webpartTitle}</h2>
            }
          </div>
          <div className="header-actions">
            {(this.props.webpartMode !== WebpartMode.contentonly) &&
              [<Button buttonType={ButtonTypes.Search} onClick={() => { this.props.buttonClick("Search"); }} disabled={false} selected={this.props.panelOpen === "Search"} />,
              <Button buttonType={ButtonTypes.Link} onClick={() => { this.props.buttonClick("Link"); }} disabled={false} selected={this.props.panelOpen === "Link"} />]
            }
            <Button buttonType={ButtonTypes.Burger} onClick={() => { this.props.buttonClick("Burger"); }} disabled={(this.props.template !== Templates.Playlist && this.props.userSecurity === Roles.Visitors)} selected={this.props.panelOpen === "Burger"} />
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
