import * as React from 'react';
import { Logger, LogLevel } from '@pnp/logging';
import "@pnp/polyfill-ie11";
import * as lodash from 'lodash';
import { IAsset } from '../../../common/models/Models';
import styles from "../../../common/CustomLearningCommon.module.scss";
import { CustomWebpartSource } from '../../../common/models/Enums';

export interface IAssetViewProps {
  asset: IAsset;
  assets: IAsset[];
  selectAsset: (assetId: string) => void;
}

export interface IAssetViewState {
}

export class AssetViewState implements IAssetViewState {
  constructor() { }
}

export default class AssetView extends React.Component<IAssetViewProps, IAssetViewState> {
  private LOG_SOURCE = "AssetView";
  private iFrame;
  private iFrameCont;

  constructor(props) {
    super(props);
    this.state = new AssetViewState();
    this.iFrame = React.createRef();
    this.iFrameCont = React.createRef();
  }

  public shouldComponentUpdate(nextProps: Readonly<IAssetViewProps>, nextState: Readonly<IAssetViewState>) {
    try {
      if ((lodash.isEqual(nextState, this.state) && lodash.isEqual(nextProps, this.props)))
        return false;
      if (!nextProps.asset)
        return false;
      if ((!this.props.asset && nextProps.asset) || (nextProps.asset && (nextProps.asset.Url != this.props.asset.Url))) {
        //Reset iFrame height
        let iFrameCont = (document.getElementsByClassName(styles.outerframe))[0] as HTMLElement;
        if (iFrameCont)
          iFrameCont.style.height = "0px";
      }
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (shouldComponentUpdate)`, LogLevel.Error);
    }
    return true;
  }

  public componentDidUpdate() {
    let url = (this.props.asset.Source === CustomWebpartSource.Tenant) ? this.props.asset.Url : `${this.props.asset.Url}&fromOrigin=${document.location.origin}`;
    this.iFrame.current.contentWindow.location.replace(url);
  }
  
  public componentDidMount() {
    //get iframe content size
    window.addEventListener("message", this.handleIFrameSize, false);
  }

  public componentWillUnmount() {
    //Release windows events
    window.removeEventListener("message", this.handleIFrameSize);
  }

  private handleIFrameSize = (event: MessageEvent) => {
    if (event.origin !== "https://support.office.com" && event.origin !== "https://docs.microsoft.com")
      return;
    let height = event.data;
    this.iFrameCont.current.style.height = height + "px";
  }

  private resizeIFrame = (): void => {
    try {
      let height: number = 897;
      try {
        let iFrameDoc = this.iFrame.current.contentDocument ? this.iFrame.current.contentDocument : this.iFrame.current.contentWindow.document;
        let iFrameDocBody = iFrameDoc.getElementById("spPageCanvasContent");
        let iFrameContent = iFrameDocBody.parentElement;
        let iFrameScroll = iFrameContent.parentElement;
        iFrameScroll.className = "";
        height = Math.max(iFrameContent.scrollHeight, iFrameContent.offsetHeight, iFrameContent.clientHeight);
      } catch (err) {
        height = this.iFrameCont.scrollHeight * 2;
      }
      this.iFrameCont.current.style.height = height + "px";
      //Make sure scroll is at the top
      document.body.scrollTop = 0;
      document.documentElement.scrollTop = 0;
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (resizeIFrame)`, LogLevel.Error);
    }
  }

  public render(): React.ReactElement<IAssetViewProps> {
    try {
      if (!this.props.asset) { return null; }
      return (
        <div>
          <div ref={this.iFrameCont} className={(this.props.asset.Source === CustomWebpartSource.Tenant) ? styles.spouterframe: styles.outerframe}>
            <iframe ref={this.iFrame} scrolling="No" frameBorder="0" allowFullScreen className={styles.innerframe} onLoad={() => { this.resizeIFrame(); }}></iframe>
          </div>
        </div>
      );
    } catch (err) {
      Logger.write(`${err} - ${this.LOG_SOURCE} (render)`, LogLevel.Error);
      return null;
    }
  }
}
