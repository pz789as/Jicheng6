/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
  AppRegistry,
  StyleSheet,
  Text,
  View,
  ART,
  Image,
} from 'react-native';

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

let cv = {
  '21475/0': require('./xzw/Resources/21475/0.png'),
  '21475/1': require('./xzw/Resources/21475/1.png'),
  '21475/2': require('./xzw/Resources/21475/2.png'),
};

export default class XzwWord extends Component {
  constructor(props){
    super(props);
    this.myInfo = props.data;
    this.arrSprBh = [];
    this.tempSprBh = [];
    this.arrBushou = [];
    this.isMove = [];
    this.waitMoveTime = [];
    this.arrTempPos = [];
    this.arrTempScale = [];
    this.arrTempAngle = [];
    this.bihuaCount = 0;
    this.orgColor = 'rgb(0,0,0)';
    this.tempVec = {};
    this.warningColor = 'rgb(0,107,1)';
    this.warningIdx = 0;
    this.blnWarning = false;
    this.warningTime = 0;

    this.state = {
      blnUpdate: false
    };
    this.Init();
  }
  Init(){
    this.bihuaCount = this.myInfo.arrBihua.length;
    for(var i=0;i<this.bihuaCount;i++){
      var bh = this.myInfo.arrBihua[i];
      this.arrSprBh[i] = (
        <Image 
          source={cv[bh.picName]}
          style={{
            position: 'absolute',
            left: bh.center.x,
            right: bh.center.y
          }} 
        />
      );
    }
  }
  setUpdate(){
    this.setState({
      blnUpdate: !this.blnUpdate,
    });
  }
  componentDidMount() {
     
  }
  componentWillUnmount() {
    
  }
  
  render() {
    return (
      <View style={[styles.container, this.props.style? this.props.style : {}]}>
        {this.arrSprBh}
      </View>
    );
  }
}

const styles = StyleSheet.create({
  container: {
    justifyContent: 'center',
    alignItems: 'center',
    backgroundColor: '#F5FCFF',
  },
  welcome: {
    fontSize: 20,
    textAlign: 'center',
    margin: 10,
  },
  instructions: {
    textAlign: 'center',
    color: '#333333',
    marginBottom: 5,
  },
});
