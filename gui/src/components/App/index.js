import React, {Component} from "react";
import {
  BrowserRouter as Router,
  Switch,
  Route
} from "react-router-dom";
import * as Paths from "../../constants/routes"

import Game from "../Game"
import NametableViewer from "../NametableViewer";

export class App extends Component{
 
  componentWillMount(){
   
  }

  game(){
    return(<span>Game function</span>)
  }

  render(){
    return(
      <div>
        <Router>
          <Switch>
            <Route exact path={Paths.HOME}> <Game/> </Route>
            <Route path={Paths.NAMETABLE_VIEWER}> <NametableViewer/> </Route>
          </Switch>
        </Router>

      </div>
      )
  }
}