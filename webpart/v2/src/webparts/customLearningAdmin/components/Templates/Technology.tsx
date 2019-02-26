import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { ITechnology } from "../../../common/models/Models";
import TechnologyHeading from "../Atoms/TechnologyHeading";
import TechnologySubjectEdit from "../Atoms/TechnologySubjectEdit";
import TechnologyNav from "../Atoms/TechnologyNav";

export interface ITechnologyProps {
  technologies: ITechnology[];
  hiddenTech: string[];
  hiddenSub: string[];
  updateTechnology: (techName: string, subTech: string, exists: boolean) => void;
}

export interface ITechnologyState {
  selectedTechnology: ITechnology;
}

export class TechnologyState implements ITechnologyState {
  constructor(
    public selectedTechnology: ITechnology = null
  ) { }
}

export default class Technology extends React.Component<ITechnologyProps, ITechnologyState> {
  private LOG_SOURCE: string = "Technology";

  constructor(props) {
    super(props);
    this.state = new TechnologyState();
  }

  public shouldComponentUpdate(nextProps: Readonly<ITechnologyProps>, nextState: Readonly<ITechnologyState>) {
    if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
      return false;
    return true;
  }

  private selectTech = (selected: ITechnology): void => {
    if (!lodash.isEqual(this.state.selectedTechnology, selected)) {
      this.setState({
        selectedTechnology: selected
      });
    }
  }

  public render(): React.ReactElement<ITechnologyProps> {
    try {
      return (
        <div className="adm-content">
          <div className="adm-navsection-subcat">
            <TechnologyNav
              technologies={this.props.technologies}
              selectedId={(this.state.selectedTechnology) ? this.state.selectedTechnology.Name : ""}
              onClick={this.selectTech}
            />
          </div>
          {this.state.selectedTechnology &&
            <div className="adm-content-main">
              <TechnologyHeading
                heading={this.state.selectedTechnology.Name}
                visible={this.props.hiddenTech.indexOf(this.state.selectedTechnology.Name) < 0}
                onVisibility={this.props.updateTechnology}
              />
              <ul className="adm-content-playlist">
                {this.state.selectedTechnology.Subjects && this.state.selectedTechnology.Subjects.length > 0 && this.state.selectedTechnology.Subjects.map((subject) => {
                  return (
                    <li>
                      <TechnologySubjectEdit
                        techName={this.state.selectedTechnology.Name}
                        techSubject={subject}
                        visible={this.props.hiddenSub.indexOf(subject) < 0}
                        onVisibility={this.props.updateTechnology}
                      />
                    </li>
                  );
                })}
              </ul>
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
