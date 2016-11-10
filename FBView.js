/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

'use strict'
import React, { Component } from 'react';
import { View } from 'react-native';
import { requireNativeComponent } from 'react-native';

class FBView extends Component {
  setNativeProps(props){
    this.root.setNativeProps(props);
  }
  render() {
    return (
      <FBViewNative
        {...this.props}
        style={[
          {backgroundColor: 'transparent'},
          this.props.style,
        ]} 
        ref={(r)=>{this.root = r}}
      />
    );
  }
}

FBView.propTypes = {
};

const FBViewNative = requireNativeComponent('RCTFBView', FBView);

export default FBView;