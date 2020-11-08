import React, {Component} from "react"
import Page from "../Page";
const { ipcRenderer } = window.require('electron');

export default class Console extends Component{

  constructor(props){
    super(props);

    ipcRenderer.on('log' , (event , data) => this.onLogReceived(data));

    this.state = {
        logs: "",
    };
  }

  onLogReceived = (data) => {
    this.setState({
      logs : this.state.logs + Buffer.from(data).toString() + "\n"
    })
  };
 
  render(){
    return(
        <Page>
            <textarea readOnly={true} className={"text-area white"} value={this.state.logs}/>
        </Page>
      )
  }
}