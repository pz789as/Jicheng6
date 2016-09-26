/**
 * Sample React Native App
 * https://github.com/facebook/react-native
 * @flow
 */

import React, { Component } from 'react';
import {
} from 'react-native';

import Dimensions from 'Dimensions';
global.ScreenWidth = global.ScreenWidth || Dimensions.get('window').width;
global.ScreenHeight = global.ScreenHeight || Dimensions.get('window').height;
global.curWidth = global.curWidth || Math.min(ScreenWidth, ScreenHeight);
global.scaleWidth = global.scaleWidth || curWidth / 400;
global.minUnit = global.minUnit || ScreenWidth / 100;
global.unitDisSt = global.unitDisSt || curWidth / 25;
global.unitDisMv = global.unitDisMv || curWidth / 15;

var Utils = {
  NormalizedPointsCount: 32,
  blnGesture: false,
  matched: [],
  Dis: function (x, y){
    return Math.sqrt(x * x + y * y);
  },
  DisP: function(p1, p2){
    return Utils.Dis(p1.x - p2.x, p1.y - p2.y);
  },
  Lerp: function(a, b, f){
    if (f <= 0) return a;
    if (f >= 1) return b;
    return a + (b - a) * f;
  },
  LerpP: function(a, b, f){
    if (f <= 0) return a;
    if (f >= 1) return b;
    return {
      'x': Utils.Lerp(a.x, b.x, f),
      'y': Utils.Lerp(a.y, b.y, f)
    };
  },
  PathLength: function(points){
    var d=0;
    for(var i=1;i<points.length;i++){
      d += Utils.DisP(points[i-1], points[i]);
    }
    return d;
  },
  Resample: function(points, normalizedPointsCount){
    normalizedPointsCount = Math.max(3, normalizedPointsCount);
    var intervalLength = Utils.PathLength(points) / (normalizedPointsCount-1);
    var D = 0;
    var q = {x:0, y:0};
    var normalizedPoints = [];
    normalizedPoints.push(points[0]);
    var pointBuffer = [];
    pointBuffer = pointBuffer.concat(points);
    for(var i=1;i<pointBuffer.length;i++){
      var a = pointBuffer[i-1];
      var b = pointBuffer[i];
      var d = Utils.DisP(a, b);
      if ((D+d) > intervalLength){
        q = Utils.LerpP(a, b, (intervalLength - D) / d);
        normalizedPoints.push(q);
        pointBuffer.splice(i, 0, q);
        D = 0;
      }else{
        D += d;
      }
    }
    if (normalizedPoints.length == normalizedPointsCount - 1){
      normalizedPoints.push(pointBuffer[pointBuffer.length - 1]);
    }
    return normalizedPoints;
  },
  ResampleByLen: function(points, len){
    len = Math.max(2, len);
    var normalizedPointsCount = parseInt(Utils.PathLength(points) / len);
    if (normalizedPointsCount <= 0) {
      return null;
    }
    return Utils.Resample(points, normalizedPointsCount);
  },
  CountDistance: function(arg1, arg2){
    return Math.round(Math.sqrt(Math.pow(arg1.x - arg2.x, 2) + Math.pow(arg1.y - arg2.y, 2)));
  },
  CompareGesture: function(line1, line2){
    Utils.blnGesture = true;
    var nLine1 = Utils.Normalize(line1);
    var nLine2 = Utils.Normalize(line2);
    var e = 0.5;
    var step = Math.floor(Math.pow(nLine1.length, 1-e));
    var min = Number.MAX_VALUE;
    for(var i=0;i<nLine1.length;i+=step){
      var d1 = Utils.CloundDistance(nLine1, nLine2, i);
      var d2 = Utils.CloundDistance(nLine2, nLine1, i);
      min = Math.min(min, d1, d2);
    }
    return min;
  },
  CloundDistance: function(points1, points2, startIndex){
    var numPoints = points1.length;
    Utils.ResetMatched(numPoints);
    if (points1.length != points2.length){
      console.warn('CloundDistance points1 count != points2 count');
      return Number.MAX_VALUE;
    }
    var sum = 0;
    var i = startIndex;
    do{
      var index = -1;
      var minDistance = Number.MAX_VALUE;
      for(var j=0;j<numPoints;j++){
        if (!Utils.matched[j]){
          var distance = Utils.Dis(points1[i], points2[j]);
          if (distance < minDistance){
            minDistance = distance;
            index = j;
          }
        }
      }
      matched[index] = true;
      var weight = 1 - ((i - startIndex + points1.length)%points1.length)/points1.length;
      sum += weight * minDistance;
      i = (i+1)%points1.length;
    }while(i != startIndex);
    return sum;
  },
  ResetMatched: function(count){
    Utils.matched = [];
    for(var i=0;i<count;i++){
      Utils.matched.push(false);
    }
  },
  CompareNormal: function(line1, line2){
    Utils.blnGesture = false;
    var nLine1 = Utils.Normalize(line1);
    var nLine2 = Utils.Normalize(line2);
    return Utils.CloundNormal(nLine1, nLine2);
  },
  CloundNormal: function(points1, points2){
    if (points1.length != points2.length){
      console.warn('CloundNormal points1 count != points2 count');
      return Number.MAX_VALUE;
    }
    var sum = 0;
    for(var i=0;i<points1.length;i++){
      var distance = Utils.DisP(points1[i], points2[i]);
      sum += distance;
    }
    return sum;
  },
  Normalize: function(points){
    return Utils.Apply(points, Utils.NormalizedPointsCount);
  },
  Apply: function(inputPoints, normalizedPointsCount){
    var normalizedPoints = Utils.Resample(inputPoints, normalizedPointsCount);
    if (Utils.blnGesture){
      Utils.Scale(normalizedPoints);
      Utils.TranslateToOrigin(normalizedPoints);
    }
    return normalizedPoints;
  },
  Scale: function(points){
    var min = {x: Number.MAX_VALUE, y: Number.MAX_VALUE};
    var max = {x: Number.MIN_VALUE, y: Number.MIN_VALUE};
    for(var i=0;i<points.length;i++){
      var p = points[i];
      min.x = Math.min(min.x, p.x);
      min.y = Math.min(min.y, p.y);
      max.x = Math.max(max.x, p.x);
      max.y = Math.max(max.y, p.y);
    }
    var size = Math.max(max.x - min.x, max.y - min.y);
    var invSize = 1 / size;
    for(var i=0;i<points.length;i++){
      var p = points[i];
      p = Utils.PMulV(Utils.PsubP(p, min), invSize);
      points[i] = p;
    }
  },
  TranslateToOrigin: function(points){
    var c = Utils.Centriod(points);
    for(var i=0;i<points.length;i++){
      var p = points[i];
      p = Utils.PsubP(p - c);
      points[i] = p;
    }
  },
  Centriod: function(points){
    var c = {x:0, y:0};
    for(var i=0;i<points.length;i++){
      c = Utils.PAddP(c, points[i]);
    }
    c = Utils.PDivV(c, points.length);
    return c;
  },
  PAddP: function(a, b){
    return {x: a.x + b.x, y: a.y + b.y};
  },
  PsubP: function(a, b){
    return {x: a.x - b.x, y: a.y - b.y};
  },
  PMulV: function(a, v){
    return {x: a.x * v, y: a.y * v}; 
  },
  PDivV: function(a, v){
    if (v == 0){
      console.warn('Utils PDivV the v is zero!!!!');
      v = 1;
    }
    return {x: a.x / v, y: a.y / v};
  }
};

module.exports = Utils;