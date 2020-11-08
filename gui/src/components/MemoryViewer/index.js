import React, {Component} from "react"
import {TextField} from "@material-ui/core"
import Page from "../Page"
import PropTypes from 'prop-types';
import {List, AutoSizer} from "react-virtualized"
import {Legend} from './legend'
import * as LegendColors from '../../constants/memoryMapColours'
import {Row} from './row'
const { ipcRenderer } = window.require('electron');


export default class MemoryViewer extends Component{

    constructor(props){
        super(props);

        this.rowRenderer = this.rowRenderer.bind(this)
        this.onSearchKeyUp = this.onSearchKeyUp.bind(this);
        this.updateAnimationEnded = this.updateAnimationEnded.bind(this)

        ipcRenderer.on('memoryUpdated' , (event , data) => this.onMemoryUpdated(data));

        let mem = {}
        for(let i = 0; i < 0xFFFF; i++){
            mem[i] = {
                content: 0,
                updated: false
            }
        }

        this.state = {
            content: mem,
            scrollToIndex: null
        };
    }



    rowRenderer({ index, key, style }){
        let color = this.getBackgroundColor(index);

        return (
            <div key={key} style={style}>
                <Row 
                index={index} 
                value={this.state.content[index].content} 
                color={color} 
                isSelected={index===this.state.scrollToIndex}
                isNewUpdate={this.state.content[index].updated}
                onAnimationEnded={() => this.updateAnimationEnded(index)}/>                
            </div>            
          );
    }

    updateAnimationEnded(memoryPosition){
        let c = this.state.content;
        c[memoryPosition] = {
            content: c[memoryPosition].content,
            updated: false
        }

        this.setState({
            content: c
        })
    }

    getBackgroundColor(i){
        if(i<= 0x3FFF)
            return LegendColors.CARTRIDGE_BANK00
        if(i <= 0x7FFF)
            return LegendColors.CARTRIDGE_BANKN
        if(i <= 0x9FFF)
            return LegendColors.VRAM
        if(i <= 0xBFFF)
            return LegendColors.EXTERNAL_RAM
        if(i <= 0xDFFF)
            return LegendColors.RAM
        if(i <= 0xFDFF)
            return LegendColors.MIRRORS
        if(i <= 0xFE9F)
            return LegendColors.OAM
        if(i <= 0xFEFF)
            return LegendColors.UNUSABLE
        if(i <= 0xFF7F)
            return LegendColors.IO
        if(i <= 0xFFFE)
            return LegendColors.HRAM
        if(i <= 0xFFFF)
            return LegendColors.INTERRUPTS
    }

    renderLegend(){
        return(
            <div className={"container-column"} style={{marginTop: 10, marginBottom: 10}}>
                <div className={"container"} style={{justifyContent: 'space-between'}}>
                    <Legend name='Cartridge' color={LegendColors.CARTRIDGE_BANK00}/>
                    <Legend name='External RAM' color={LegendColors.EXTERNAL_RAM}/>
                    <Legend name='VRAM' color={LegendColors.VRAM}/>
                </div>
                <div className={"container"} style={{justifyContent: 'space-between'}}>
                    <Legend name='Mirrors' color={LegendColors.MIRRORS}/>
                    <Legend name='RAM' color={LegendColors.RAM}/>
                    <Legend name='OAM' color={LegendColors.OAM}/>
                </div>
                <div className={"container"} style={{justifyContent: 'space-between'}}>
                    <Legend name='Unusable' color={LegendColors.UNUSABLE}/>
                    <Legend name='IO' color={LegendColors.IO}/>
                    <Legend name='HRAM' color={LegendColors.HRAM}/>
                </div>
            </div>
        )
    }

    onSearchKeyUp(e){
        if(e.target.value === ''){
            this.setState({
                scrollToIndex: null
            })
        }

        let val = parseInt(e.target.value);

        //See if we're searching by hex value
        if(!isNaN(val)){
            this.setState({
                scrollToIndex: val
            })
        }
    }

    onMemoryUpdated(data){
        if(this.state === null || this.state.content === null)
            return
        let c = this.state.content;
        c[data[1]] = {
            content: data[2],
            updated: true
        }

        this.setState({
            content: c
        })
    }

    render(){
        return(
            <Page style={{marginLeft: 10, marginRight: 10}}>
                <span className={"center title"}>MEMORY  VIEWER</span>
                <TextField id="outlined-basic" size={"small"} onKeyUp={this.onSearchKeyUp} label="Search by address" variant="outlined" />

                {/** Header */}
                <div className={"container"} style={{marginTop: 10, marginBottom:5}}>
                    <span className={"container justify-center super"} style={{flex: 2}}>Address</span>
                    <span className={"container justify-center super"} style={{flex: 3}}>Content</span>
                </div>

                <div style={{ display: 'flex', flexGrow:1, border:'1px black solid'}}>  
                    <div style={{ flex: '1 1 auto' }}>
                        <AutoSizer>
                            {({height, width}) => (
                                <List
                                    height={height}
                                    rowCount={0xFFFF}
                                    rowHeight={20}
                                    rowRenderer={this.rowRenderer}
                                    width={width}
                                    scrollToIndex={this.state.scrollToIndex}
                                />
                            )}
                        </AutoSizer>                
                    </div>
                </div>

                {this.renderLegend()}
                
            </Page>
            )
    }
}

MemoryViewer.propTypes = {
    content: PropTypes.array,
    scrollToIndex: PropTypes.number
}