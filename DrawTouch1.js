/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  StyleSheet,
  View,
  ART,
} from 'react-native';

import Utils from './Utils';

const {
  Shape,
  Group,
  Transform,
  Surface,
  Path,
  Pattern,
  LinearGradient,
  RadialGradient,
  // Text,
  ClippingRectangle,
} = ART;

export default class DrawTouch extends Component {
  constructor(props){
    super(props);
    this.points = this.props.data;
    this.strokeColor = this.props.strokeColor;
    this.strokeWidth = this.props.strokeWidth;
    this.state={
      blnUpdate: false,
    };
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.state.blnUpdate
    });
  }
  setPoints(ps){
    this.points = ps;
    this.setUpdate();
  }
  render() {
    return (
      <View style={styles.mouseView}>
        <Surface ref={'lineView'} width={ScreenWidth} height={ScreenHeight}>
          <Shape d={this.points} stroke={this.strokeColor} strokeWidth={this.strokeWidth} />
        </Surface>
      </View>
    );
  }
}

const styles = StyleSheet.create({
  mouseView:{
    position: 'absolute',
    left: 0,
    top: 0,
    width: ScreenWidth,
    height: ScreenHeight,
    backgroundColor: 'transparent'
  },
});
