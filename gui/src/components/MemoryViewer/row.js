import React, {Component} from "react";
import PropTypes from 'prop-types';
import './row.scss'
import AnimateOnChange from 'react-animate-on-change'

export class Row extends Component{

    pad(n, width, z) {
        z = z || '0';
        n = n + '';
        return n.length >= width ? n : new Array(width - n.length + 1).join(z) + n;
      }

    render(){        

        let isIndexSelectedClass = this.props.isSelected ? "bold large" : ''

        return(
            <div className={"container " + isIndexSelectedClass} style={{backgroundColor:this.props.color, paddingBottom:2}}>
                <span className={"container justify-center"} style={{flex: 2}}>
                        0x{this.pad(this.props.index.toString(16), 4).toUpperCase()}
                </span>
                <div className={"container justify-center"} style={{flex: 3}}>
                    <AnimateOnChange
                        animationClassName="content"
                        onAnimationEnd={this.props.onAnimationEnded}
                        animate={this.props.isNewUpdate}>
                        <span>0x{this.pad(this.props.value,4).toUpperCase()}</span>
                    </AnimateOnChange>
                </div>
            </div>)
    }
}

Row.propTypes = {
    index: PropTypes.number, 
    value: PropTypes.number,
    isSelected: PropTypes.bool,
    color: PropTypes.string,
    onAnimationEnded: PropTypes.func,
    isNewUpdate: PropTypes.bool
}
