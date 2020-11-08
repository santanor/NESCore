import React, { Component } from "react"
import "../../style.scss"

export default class Page extends Component{

    render(){
        return(
            <div className={"page"} style={this.props.style}>
                {this.props.children}
            </div>
        );
    }
}