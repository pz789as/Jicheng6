//
//  RCTFBView.h
//  Jicheng6
//
//  Created by guojicheng on 16/11/10.
//  Copyright © 2016年 Facebook. All rights reserved.
//

#import <UIKit/UIKit.h>
#import <FBSDKLoginKit/FBSDKLoginKit.h>

@interface RCTFBView : UIView<FBSDKLoginButtonDelegate>

@property (nonatomic, strong)FBSDKLoginButton* loginButton;

@end
