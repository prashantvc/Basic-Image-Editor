using System;
using Android.App;
using Android.Widget;
using Android.Graphics;
using Android.OS;
using Android.Views;
using Android.Content;
using Android.Util;

namespace ImageEditor
{
    [Activity(Label = "ImageEditor", MainLauncher = true)]
    public class Activity1 : Activity
    {
        ImageView _imageViewer;
        ColorMatrix _colorMatrix;
        ColorMatrixColorFilter _colorMatrixFilter;
        Paint _paint;
        Canvas _canvas;
        Bitmap _originalBitmap;
        Bitmap _canvasBitmap;
        bool _imageSelected;

        protected override void OnCreate(Bundle bundle)
        {
            base.OnCreate(bundle);
            RequestWindowFeature(WindowFeatures.NoTitle);

            SetContentView(Resource.Layout.Main);


            _colorMatrix = new ColorMatrix();
            _colorMatrixFilter = new ColorMatrixColorFilter(_colorMatrix);

            _paint = new Paint();
            _paint.SetColorFilter(_colorMatrixFilter);


            var contrast = FindViewById<SeekBar>(Resource.Id.contrast);
            contrast.KeyProgressIncrement = 1;
            contrast.ProgressChanged += ContrastChanged;
           
            var brightness = FindViewById<SeekBar>(Resource.Id.brightness);
            brightness.KeyProgressIncrement = 1;
            brightness.ProgressChanged += BrightnessChanged;

            var saturation = FindViewById<SeekBar>(Resource.Id.saturation);
            saturation.KeyProgressIncrement = 1;
            saturation.ProgressChanged += SaturationChanged;


            var imageSelector = FindViewById<Button>(Resource.Id.selectImage);
            imageSelector.Click += ImagesSelectorClick;

            _imageViewer = FindViewById<ImageView>(Resource.Id.imageViewer);

        }

        void ImagesSelectorClick(object sender, EventArgs e)
        {
            var intent = new Intent();
            intent.SetType("image/*");
            intent.SetAction(Intent.ActionGetContent);
            StartActivityForResult(Intent.CreateChooser(intent, "Select Picture"), 1);
        }

        protected override void OnActivityResult(int requestCode, Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && requestCode == 1)
            {
                _imageSelected = true;

                Android.Net.Uri selectedImageUri = data.Data;

                _originalBitmap = BitmapFactory.DecodeStream(ContentResolver.OpenInputStream(selectedImageUri));

                //Create a new mutable Bitmap from original  
                _canvasBitmap = Bitmap.CreateBitmap(_originalBitmap.Width, _originalBitmap.Height, _originalBitmap.GetConfig());

                //Initialize the canvas assigning the mutable Bitmap to it  
                _canvas = new Canvas(_canvasBitmap);

                SetBitmapToImageViewer();
            }
        }

        void SaturationChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!_imageSelected)
            {
                return;
            }

            float value = e.Progress / 100f;
            _colorMatrix.SetSaturation(value);
            UpdateImage();
        }

        void BrightnessChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!_imageSelected)
            {
                return;
            }

            var value = GetAdjustedValue(e);
            var brightness = value / (float)100;

            _colorMatrix.Set(new float[] {
                1, 0, 0, brightness, 0,
                0, 1, 0, brightness, 0,
                0, 0, 1, brightness, 0,
                0, 0, 0, 1, 0 });

            UpdateImage(); 
        }

        void ContrastChanged(object sender, SeekBar.ProgressChangedEventArgs e)
        {
            if (!_imageSelected)
            {
                return;
            }

            var value = GetAdjustedValue(e);
            var input = value / (float)100;

            float scale = input + 1f;
            float contrast = (-0.5f * scale + 0.5f) * 255f;
            _colorMatrix.Set(new float[] {
                1, 0, 0, 0, contrast,
                0, 1, 0, 0, contrast,
                0, 0, 1, 0, contrast,
                0, 0, 0, 1, 0 });

            UpdateImage(); 
        }

        void UpdateImage()
        {
            _colorMatrixFilter = new ColorMatrixColorFilter(_colorMatrix);
            _paint.SetColorFilter(_colorMatrixFilter);
            SetBitmapToImageViewer();
        }

        void SetBitmapToImageViewer()
        {   
            //Draw the Bitmap into the mutable Bitmap using the canvas. 
            _canvas.DrawBitmap(_originalBitmap, new Matrix(), _paint);

            //Set the mutalbe Bitmap to be rendered by the ImageView  
            _imageViewer.SetImageBitmap(_canvasBitmap); 
        }

        int GetAdjustedValue(SeekBar.ProgressChangedEventArgs e)
        {
            return e.Progress - 100;
        }

        void Debug(string message)
        {
            Log.Debug("image editor", message);
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();

            if (_paint != null)
            {
                _paint.Dispose();
            }
        }
    }
}


