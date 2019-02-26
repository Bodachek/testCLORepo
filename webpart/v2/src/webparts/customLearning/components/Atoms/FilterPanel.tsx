import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import Button from "../../../common/components/Atoms/Button";
import { ButtonTypes, FilterTypes } from "../../../common/models/Enums";
import TextButton from "../../../common/components/Atoms/TextButton";
import { IFilter, IFilterValue } from "../../../common/models/Models";
import * as strings from "CustomLearningWebPartStrings";

export interface IFilterPanelProps {
  filter: IFilter;
  filterValues: IFilterValue[];
  setFilter: (filterValue: IFilterValue) => void;
}

export interface IFilterPanelState {
  show: boolean;
}

export class FilterPanelState implements IFilterPanelState {
  constructor(
    public show: boolean = false
  ) { }
}

export default class FilterPanel extends React.Component<IFilterPanelProps, IFilterPanelState> {
  private LOG_SOURCE: string = "FilterPanel";

  constructor(props) {
    super(props);
    this.state = new FilterPanelState();
  }

  public shouldComponentUpdate(nextProps: Readonly<IFilterPanelProps>, nextState: Readonly<IFilterPanelState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  public showFilter = (): void => {
    let show = this.state.show;
    this.setState({
      show: !show
    });
  }

  public render(): React.ReactElement<IFilterPanelProps> {
    try {
      let filterValuesLevel = lodash.filter(this.props.filterValues, { Key: FilterTypes.Level });
      let filterValuesAudience = lodash.filter(this.props.filterValues, { Key: FilterTypes.Audience });
      return (
        <div className={`sldpnl ${this.state.show ? "show" : "hide"}`}>
          <div className="sldpnl-header">
            <div className="sldpnl-title">
              {strings.FilterPanelHeader}
            </div>
            <div className="sldpnl-toggle">
              <Button buttonType={(this.state.show) ? ButtonTypes.ChevronUp : ButtonTypes.ChevronDown} disabled={false} onClick={() => { this.showFilter(); }} />
            </div>
          </div>
          <div className="sldpnl-content">
            <table className="selector">
              <tr className="selector-row">
                <th className="selector-header">{strings.FilterPanelAudienceLabel}</th>
                <td className="selector-data">
                  {filterValuesAudience.map((audience) => {
                    return (
                      <TextButton onClick={() => { this.props.setFilter(audience); }} label={audience.Value} selected={this.props.filter.Audience.indexOf(audience.Value) > -1} />
                    );
                  })}
                </td>
              </tr>
              <tr className="selector-row">
                <th className="selector-header">{strings.FilterPanelSkillsetLabel}</th>
                <td className="selector-data">
                  {filterValuesLevel.map((level) => {
                    return (
                      <TextButton onClick={() => { this.props.setFilter(level); }} label={level.Value} selected={this.props.filter.Level.indexOf(level.Value) > -1} />
                    );
                  })}
                </td>
              </tr>
            </table>
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
