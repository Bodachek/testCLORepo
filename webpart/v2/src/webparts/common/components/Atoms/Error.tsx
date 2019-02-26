import * as React from "react";
import { Logger, LogLevel } from "@pnp/logging";
import "@pnp/polyfill-ie11";
import * as lodash from "lodash";
import { MessageBar, MessageBarType } from "office-ui-fabric-react/lib/MessageBar";

export interface IErrorProps {
  message: string;
}

export interface IErrorState {
}

export class ErrorState implements IErrorState {
constructor() {}
}

export default class Error extends React.Component<IErrorProps, IErrorState> {
 private LOG_SOURCE: string = "Error";

 constructor(props){
  super(props);
  this.state = new ErrorState();
 }

 public shouldComponentUpdate(nextProps: Readonly<IErrorProps>, nextState: Readonly<IErrorState>){
  if((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
   return false;
  return true;
 }

 public render(): React.ReactElement<IErrorProps> {
  return (
    <MessageBar messageBarType={MessageBarType.error}>
    {this.props.message}
    </MessageBar>
  );
 }

}