﻿//   
// Copyright (c) Jesse Freeman, Pixel Vision 8. All rights reserved.  
//  
// Licensed under the Microsoft Public License (MS-PL) except for a few
// portions of the code. See LICENSE file in the project root for full 
// license information. Third-party libraries used by Pixel Vision 8 are 
// under their own licenses. Please refer to those libraries for details 
// on the license they use.
// 
// Contributors
// --------------------------------------------------------
// This is the official list of Pixel Vision 8 contributors:
//  
// Jesse Freeman - @JesseFreeman
// Christina-Antoinette Neofotistou @CastPixel
// Christer Kaitila - @McFunkypants
// Pedro Medeiros - @saint11
// Shawn Rakowski - @shwany
//

using System;

namespace PixelVision8.Player
{

    public partial class Utilities
    {
        public static void FlipPixelData(ref int[] pixelData, int sWidth, int sHeight, bool flipH = false,
            bool flipV = false)
        {
            var total = pixelData.Length;
            
            var pixels = new int[total];

            // if (pixels.Length < total) Array.Resize(ref pixels, total);

            Array.Copy(pixelData, pixels, total);

            for (var ix = 0; ix < sWidth; ix++)
            for (var iy = 0; iy < sHeight; iy++)
            {
                var newx = ix;
                var newY = iy;
                if (flipH) newx = sWidth - 1 - ix;

                if (flipV) newY = sHeight - 1 - iy;

                pixelData[ix + iy * sWidth] = pixels[newx + newY * sWidth];
            }
        }
    }
    
    /// <summary>
    ///     The GameChip represents the foundation of a game class
    ///     with all the logic it needs to work correctly in the PixelVisionEngine.
    ///     The AbstractChip class manages configuring the game when created via the
    ///     chip life-cycle. The engine manages the game's state, the game's own life-cycle and
    ///     serialization/deserialization of the game's data.
    /// </summary>
    public partial class GameChip
    {
        
        protected SpriteData _currentSpriteData;
        protected SpriteCollection[] metaSprites = new SpriteCollection[256];
        public int maxSize = 256;

        public int TotalMetaSprites => metaSprites.Length;
        
        /// <summary>
        ///     Returns the total number of sprites in the system. You can pass in an optional argument to
        ///     get a total number of sprites the Sprite Chip can store by passing in false for ignoreEmpty.
        ///     By default, only sprites with pixel data will be included in the total return.
        /// </summary>
        /// <param name="ignoreEmpty">
        ///     This is an optional value that defaults to true. When set to true, the SpriteChip returns
        ///     the total number of sprites that are not empty (where all the pixel data is set to -1).
        ///     Set this value to false if you want to get all of the available color slots in the ColorChip
        ///     regardless if they are empty or not.
        /// </param>
        /// <returns>
        ///     This method returns the total number of sprites in the color chip based on the ignoreEmpty
        ///     argument's value.
        /// </returns>
        public int TotalSprites(bool ignoreEmpty = false)
        {
            return ignoreEmpty ? SpriteChip.SpritesInMemory : SpriteChip.TotalSprites;
        }

        /// <summary>
        ///     This method returns the maximum number of sprites the Display Chip can render in a single frame. Use this
        ///     to better understand the limitations of the hardware your game is running on. This is a read only property
        ///     at runtime.
        /// </summary>
        /// <param name="total"></param>
        /// <returns>Returns an int representing the total number of sprites on the screen at once.</returns>
        public int MaxSpriteCount()
        {
            return SpriteChip.MaxSpriteCount;
        }

        public SpriteCollection MetaSprite(string name, SpriteCollection spriteCollection = null)
        {
            return MetaSprite(FindMetaSpriteId(name), spriteCollection);
        }

        public SpriteCollection MetaSprite(int id, SpriteCollection spriteCollection = null)
        {
            if (id < 0 || id > metaSprites.Length)
                return null;

            if (spriteCollection != null)
                metaSprites[id] = spriteCollection;
            else if (metaSprites[id] == null)
            {
                metaSprites[id] =
                    new SpriteCollection("EmptyMetaSprite")
                    {
                        SpriteWidth = 0,
                        SpriteHeight = 0,
                        SpriteMax = 256,
                    };
            }
                

            return metaSprites[id];
        }

        public int FindMetaSpriteId(string name)
        {
            var total = metaSprites.Length;
            
            // Loop through all of the meta sprites and find a name that matches
            for (int i = 0; i < total; i++)
            {
                if (metaSprites[i] != null && metaSprites[i].Name == name)
                    return i;
            }

            // If no match is found
            return -1;
        }

        public void DrawMetaSprite(string name, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, SpriteChip srcChip = null)
        {
            DrawMetaSprite(FindMetaSpriteId(name), x, y, flipH, flipV, drawMode, colorOffset);
        }

        public void DrawMetaSprite(int id, int x, int y, bool flipH = false, bool flipV = false,
            DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0, SpriteChip srcChip = null)
        {
            // This draw method doesn't support background or tile draw modes
            if (id == -1) return;

            DrawMetaSprite(metaSprites[id], x, y, flipH, flipV, drawMode, colorOffset);

        }

        public void DrawMetaSprite(SpriteCollection spriteCollection, int x, int y, bool flipH = false, bool flipV = false, DrawMode drawMode = DrawMode.Sprite, int colorOffset = 0)
        {

            // Get the sprite data for the meta sprite
            var tmpSpritesData = spriteCollection.Sprites;
            var total = tmpSpritesData.Count;
                
            int startX, startY;
            bool tmpFlipH, tmpFlipV;

            var spriteSize = Constants.SpriteSize;
            var width = spriteCollection.Bounds.Width;
            var height = spriteCollection.Bounds.Height;
            
            // Loop through each of the sprites
            for (var i = 0; i < total; i++)
            {
                _currentSpriteData = tmpSpritesData[i];

                if (_currentSpriteData.Id != -1)
                {
                    // Get sprite values
                    startX = _currentSpriteData.X;
                    startY = _currentSpriteData.Y;
                    tmpFlipH = _currentSpriteData.FlipH;
                    tmpFlipV = _currentSpriteData.FlipV;

                    if (flipH)
                    {
                        startX = width - startX - spriteSize;
                        tmpFlipH = !tmpFlipH;
                    }

                    if (flipV)
                    {
                        startY = height - startY - spriteSize;
                        tmpFlipV = !tmpFlipV;
                    }

                    if (drawMode == DrawMode.Tile){
                    
                        // Get sprite values
                        startX = (int)Math.Ceiling((double)startX/ spriteSize);
                        startY = (int)Math.Ceiling((double)startY / spriteSize);

                    }

                    startX += x;
                    startY += y;

                    DrawSprite(
                        _currentSpriteData.Id,
                        startX,
                        startY,
                        tmpFlipH,
                        tmpFlipV,
                        drawMode,
                        _currentSpriteData.ColorOffset + colorOffset
                    );
                }
            }
        }
    }
}