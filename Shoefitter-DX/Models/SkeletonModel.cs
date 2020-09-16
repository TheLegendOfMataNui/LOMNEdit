﻿using SharpDX;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Text;

namespace ShoefitterDX.Models
{
    public class BoneModel : INotifyPropertyChanged
    {
        private uint _id;
        public uint ID
        {
            get => this._id;
            set
            {
                this._id = value;
                this.RaisePropertyChanged(nameof(ID));
            }
        }

        private Matrix _transform;
        public Matrix Transform
        {
            get => this._transform;
            set
            {
                this._transform = value;
                this.RaisePropertyChanged(nameof(Transform));
            }
        }

        public ObservableCollection<BoneModel> Children { get; } = new ObservableCollection<BoneModel>();

        public BoneModel(uint id, Matrix transform)
        {
            this.ID = id;
            this.Transform = transform;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }

    public class SkeletonModel : INotifyPropertyChanged
    {
        private bool _isBiped = false;
        public bool IsBiped
        {
            get => this._isBiped;
            set
            {
                this._isBiped = value;
                this.RaisePropertyChanged(nameof(IsBiped));
            }
        }

        public ObservableCollection<BoneModel> RootBones { get; } = new ObservableCollection<BoneModel>();

        public SkeletonModel(bool isBiped)
        {
            this.IsBiped = isBiped;
        }

        public SkeletonModel(SAGESharp.BHDFile bhdFile, bool isBiped)
        {
            this.IsBiped = isBiped;
            this.RootBones.Add(this.ImportBHDBone(bhdFile.Bones[0]));
        }

        private void WriteBoneTransform(BoneModel bone, Matrix[] outputArray)
        {
            outputArray[bone.ID] = bone.Transform;
            foreach (BoneModel childBone in bone.Children)
            {
                WriteBoneTransform(childBone, outputArray);
            }
        }

        public void WriteTransforms(Matrix[] outputArray)
        {
            foreach (BoneModel rootBone in RootBones)
            {
                WriteBoneTransform(rootBone, outputArray);
            }
        }

        private void BakeBonePose(BoneModel bone, Matrix transform, Matrix[] poses, Matrix[] outputTransforms)
        {
            outputTransforms[bone.ID] = poses[bone.ID] * transform;
            foreach (BoneModel childBone in bone.Children)
            {
                BakeBonePose(childBone, outputTransforms[bone.ID], poses, outputTransforms);
            }
        }

        public void BakePose(Matrix[] poses, Matrix[] outputTransforms)
        {
            foreach (BoneModel rootBone in RootBones)
            {
                BakeBonePose(rootBone, Matrix.Identity, poses, outputTransforms);
            }
        }

        private BoneModel ImportBHDBone(SAGESharp.BHDFile.Bone b)
        {
            BoneModel bone = new BoneModel(b.Index, b.Transform);
            //bone.Transform.Transpose();

            foreach (SAGESharp.BHDFile.Bone childBone in b.Children)
            {
                bone.Children.Add(this.ImportBHDBone(childBone));
            }

            return bone;
        }

        #region INotifyPropertyChanged
        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
}
