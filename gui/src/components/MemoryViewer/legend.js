import React from 'react'


export const Legend = ({ name, color }) => {
    return (
      <div className={"container"}>
        <div style={{width: 12, height: 12, backgroundColor:color, margin:"auto", marginRight: 5}}/>
        <span>{name}</span>
      </div>
    );
};