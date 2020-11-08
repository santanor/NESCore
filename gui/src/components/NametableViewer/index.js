import React, {Component} from "react"
import Page from "../Page"
import PropTypes from 'prop-types';
const { ipcRenderer } = window.require('electron');


export default class NametableViewer extends Component{

    constructor(props){
        super(props);

        this.state = {
        };
    }
    render(){
        return(
            <Page >
                

            </Page>
        )
    }
}

NametableViewer.propTypes = {
    content: PropTypes.array,
    scrollToIndex: PropTypes.number
}